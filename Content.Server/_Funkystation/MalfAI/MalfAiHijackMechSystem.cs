// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Timing;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Stunnable;
using Content.Shared.CombatMode;
using Robust.Shared.Containers;
using Content.Server.Actions;
using Content.Server.Silicons.StationAi;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared._Gabystation.MalfAi.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Malf AI Hijack Mech
/// - Targets a fully built mech. If piloted, ejects and stuns the pilot.
/// - Inserts the AI as the mech pilot.
/// - configurable duration for when the AI is returned to their core automatically.
/// - If the AI uses the mech eject ability (or is otherwise ejected), they are returned to core immediately.
/// </summary>
public sealed class MalfAiHijackMechSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMechSystem _mech = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    private static readonly TimeSpan HijackDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PilotStun = TimeSpan.FromSeconds(8);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfunctioningAiComponent, MalfAiHijackMechActionEvent>(OnHijackMechMarker);

        // Also handle cases where the AI is removed from any container (e.g., forced eject) by listening on the AI entity itself.
        // This avoids duplicate subscriptions with SharedMechSystem which already listens on MechPilotComponent for the same event.
        SubscribeLocalEvent<StationAiHeldComponent, EntGotRemovedFromContainerMessage>(OnAiRemovedFromContainer);
        // While hijacking, Return to Core should directly trigger ReturnFromHijack even if the AI lacks StationAiHeld.
        SubscribeLocalEvent<MalfAiMechHijackComponent, MalfAiReturnToCoreActionEvent>(OnReturnToCoreWhileHijacked);

    }

    private void OnHijackMechMarker(EntityUid uid, MalfunctioningAiComponent comp, ref MalfAiHijackMechActionEvent args)
    {
        // Fallback handler if the event was raised on an AI entity that might not carry StationAiHeldComponent in the subscription context.
        HandleHijack(uid, ref args);
    }

    private void OnHijackMech(Entity<StationAiHeldComponent> ai, ref MalfAiHijackMechActionEvent args)
    {
        HandleHijack(ai.Owner, ref args);
    }

    private void HandleHijack(EntityUid aiUid, ref MalfAiHijackMechActionEvent args)
    {
        var popupTarget = GetAiEyeForPopup(aiUid) ?? aiUid;

        // Must be Malfunctioning AI
        if (!HasComp<MalfunctioningAiComponent>(aiUid))
            return;

        // Validate target mech (entity-targeted)
        var target = args.Target;
        if (!TryComp<MechComponent>(target, out var mech))
        {
            _popup.PopupEntity(Loc.GetString("malfai-hijack-invalid-target"), popupTarget, aiUid);
            return;
        }

        if (mech.Broken)
        {
            _popup.PopupEntity(Loc.GetString("mech-no-enter", ("item", target)), popupTarget, aiUid);
            return;
        }

        // Determine which entity will actually pilot: the AI's positronic brain (entity with StationAiHeldComponent).
        var pilotUid = aiUid;
        if (!HasComp<StationAiHeldComponent>(pilotUid))
        {
            _popup.PopupEntity(Loc.GetString("malfai-hijack-invalid-target"), popupTarget, aiUid);
            return;
        }

        // Eject existing pilot if present, then stun them
        if (mech.PilotSlot.ContainedEntity is EntityUid currentPilot)
        {
            _mech.TryEject(target, mech);
            _stun.TryParalyze(currentPilot, PilotStun, true);
            _popup.PopupEntity(Loc.GetString("mech-eject-pilot-alert", ("item", target), ("user", aiUid)), popupTarget, aiUid);
        }

        // Determine and record the core holder BEFORE moving the brain, so we can return later.
        var hijack = EnsureComp<MalfAiMechHijackComponent>(pilotUid);
        hijack.HijackedMech = target;
        hijack.CoreHolder = TryGetAiCoreHolder(pilotUid, out var coreHolder) ? coreHolder : null;
        Dirty(pilotUid, hijack);

        // Remove the AI brain from its current holder container (core/APC) before inserting into mech.
        BaseContainer? previousContainer = null;
        if (_containers.TryGetContainingContainer(pilotUid, out var containing))
        {
            previousContainer = containing;
            _containers.Remove(pilotUid, containing);
        }

        // Try to insert the AI brain as pilot
        var inserted = _mech.TryInsert(target, pilotUid, mech);
        if (!inserted)
        {
            var pilotPopupTarget = GetAiEyeForPopup(pilotUid) ?? pilotUid;
            _popup.PopupEntity(Loc.GetString("malfai-hijack-insert-failed"), pilotPopupTarget, pilotUid);
            RemComp<MalfAiMechHijackComponent>(pilotUid);
            return;
        }

        // Grant Return-to-Core during hijack, remember the action entity for cleanup.
        if (hijack.ReturnAction == null)
        {
            var returnAction = _actions.AddAction(pilotUid, "ActionMalfAiReturnToCore");
            if (returnAction != null)
                hijack.ReturnAction = returnAction.Value;
        }

        // Ensure CombatModeComponent exists while hijacking so the toggle action functions.
        if (!TryComp<CombatModeComponent>(pilotUid, out _))
        {
            EnsureComp<CombatModeComponent>(pilotUid);
            hijack.AddedCombatComp = true;
        }

        Timer.Spawn(HijackDuration, () => ReturnFromHijack(pilotUid));

        var successPopupTarget = GetAiEyeForPopup(pilotUid) ?? pilotUid;
        _popup.PopupEntity(Loc.GetString("malfai-hijack-success"), successPopupTarget, pilotUid);
        args.Handled = true;
    }


    private void OnReturnToCoreWhileHijacked(EntityUid uid, MalfAiMechHijackComponent comp, ref MalfAiReturnToCoreActionEvent args)
    {
        // Only handle if actually hijacking; otherwise let other systems process.
        if (args.Handled)
            return;
        // uid here is the AI entity because the action is granted to the AI while hijacking.

        ReturnFromHijack(uid);
        args.Handled = true;
    }

    private void OnAiRemovedFromContainer(EntityUid uid, StationAiHeldComponent comp, EntGotRemovedFromContainerMessage args)
    {
        // If a hijacking AI gets removed from any container (e.g., forced eject), return them to core.
        if (!HasComp<MalfAiMechHijackComponent>(uid))
            return;

        ReturnFromHijack(uid);
    }

    private bool TryGetAiCoreHolder(EntityUid ai, [NotNullWhen(true)] out EntityUid? coreHolder)
    {
        coreHolder = default;
        // Try to find AI core.
        if (_stationAi.TryGetCore(ai, out var core) && core.Comp != null)
        {
            coreHolder = core.Owner;
            return true;
        }
        return false;
    }

    public void ReturnFromHijack(EntityUid ai)
    {
        if (!TryComp<MalfAiMechHijackComponent>(ai, out var hijack))
            return;

        // Remove any temporary Return-to-Core action granted during hijack.
        if (hijack.ReturnAction != null)
        {
            try { _actions.RemoveAction(hijack.ReturnAction.Value); }
            catch { }
            hijack.ReturnAction = null;
        }

        // Remove from current container if any (e.g., mech pilot slot).
        if (_containers.TryGetContainingContainer(ai, out var container))
        {
            _containers.Remove(ai, container);
        }

        // Insert back into the AI core holder if possible; if invalid, eject to floor and notify.
        if (hijack.CoreHolder != null && !Deleted(hijack.CoreHolder.Value) && HasComp<StationAiCoreComponent>(hijack.CoreHolder.Value))
        {
            var holder = hijack.CoreHolder.Value;
            var coreContainer = _containers.EnsureContainer<ContainerSlot>(holder, StationAiHolderComponent.Container);
            if (coreContainer.ContainedEntities.Count == 0)
            {
                _containers.Insert(ai, coreContainer);
            }
            else
            {
                // Fallback: if occupied, just drop the AI next to the core holder.
                var xforms = EntityManager.System<SharedTransformSystem>();
                xforms.DropNextTo(ai, holder);
            }
        }
        else
        {
            // Core missing/invalid: already removed from container above; make sure it's on the floor near where it was.
            // If we had a container we removed from, it already left; otherwise no-op. Notify the AI.
            var popupTargetCoreMissing = GetAiEyeForPopup(ai) ?? ai;
            _popup.PopupEntity("Core not found!", popupTargetCoreMissing, ai);
        }

        // Ensure the AI is marked as held and clean up state.
        EnsureComp<StationAiHeldComponent>(ai);


        // Remove combat mode from the AI
        if (hijack.AddedCombatComp)
        {
            RemComp<CombatModeComponent>(ai);
            hijack.AddedCombatComp = false;
        }

        RemComp<MalfAiMechHijackComponent>(ai);

        // Small UX: notify on successful return when core existed.
        if (hijack.CoreHolder != null && !Deleted(hijack.CoreHolder.Value) && HasComp<StationAiCoreComponent>(hijack.CoreHolder.Value))
        {
            var popupTarget = GetAiEyeForPopup(ai) ?? ai;
            _popup.PopupEntity(Loc.GetString("malfai-return-success"), popupTarget, ai);
        }
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}
