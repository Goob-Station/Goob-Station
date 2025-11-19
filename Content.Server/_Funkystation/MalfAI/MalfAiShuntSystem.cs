// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Server.Power.Components;
using Content.Server.Actions;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Content.Server.Silicons.StationAi;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Server._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.MalfAI;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles Malf AI shunting of the AI brain to APCs, and returning to the core.
/// Moves the entity with StationAiCoreComponent between container slots on the core/APC holders.
/// </summary>
public sealed class MalfAiShuntSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe on the AI performer entity.
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiShuntToApcActionEvent>(OnShuntToApc);
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiReturnToCoreActionEvent>(OnReturnToCore);
    }

    private void OnShuntToApc(Entity<StationAiHeldComponent> ai, ref MalfAiShuntToApcActionEvent args)
    {
        var popupTarget = GetAiEyeForPopup(ai.Owner) ?? ai.Owner;

        // Only Malf AI can shunt.
        if (!HasComp<MalfunctioningAiComponent>(ai))
            return;

        if (!HasComp<ApcComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("malfai-shunt-invalid-target"), popupTarget, ai);
            return;
        }
        var target = args.Target;

        // Ensure the AI is currently inside a holder (core or other) and get that holder.
        if (!_containers.TryGetContainingContainer((ai.Owner, null, null), out var currentContainer) || currentContainer is not ContainerSlot)
        {
            _popup.PopupEntity(Loc.GetString("malfai-shunt-no-holder"), popupTarget, ai);
            return;
        }

        var currentHolder = currentContainer.Owner;

        // Prepare destination holder on the APC.
        // Ensure the APC has a suitable slot/container to hold the AI brain.
        var destContainer = _containers.EnsureContainer<ContainerSlot>(target, StationAiHolderComponent.Container);

        // Ensure APC can be interacted with via intellicard by adding a StationAiHolderComponent dynamically.
        // This links the existing container to an ItemSlot so the standard intellicard transfer logic works.
        EnsureComp<StationAiHolderComponent>(target);

        // Edge case popup for if there's another malf AI occupying. Which probably will never happen, but still.
        if (destContainer.ContainedEntities.Count != 0)
        {
            _popup.PopupEntity(Loc.GetString("malfai-shunt-apc-occupied"), popupTarget, ai);
            return;
        }

        // Remember the original core holder for return if we haven't recorded it yet.
        var shunted = EnsureComp<MalfAiShuntedComponent>(ai);
        if (shunted.CoreHolder == null)
        {
            // The first shunt: assume the current holder is the AI core.
            shunted.CoreHolder = currentHolder;
        }

        // Move the AI brain to the APC.
        _containers.Remove(ai.Owner, currentContainer);
        _containers.Insert(ai.Owner, destContainer);

        // Ensure the AI stays marked as held.
        EnsureComp<StationAiHeldComponent>(ai);

        // --- Begin UI cleanup logic moved from MalfAiViewportSystem ---
        if (TryComp<MalfAiViewportComponent>(ai, out var comp))
            comp.Selected = null;

        if (TryComp<ActorComponent>(ai, out var actor) && actor.PlayerSession != null)
            RaiseNetworkEvent(new MalfAiViewportCloseEvent(), actor.PlayerSession);
        // --- End UI cleanup logic ---

        // Grant Return to Core action while shunted, and remember the action entity for removal.
        if (shunted.ReturnAction == null)
        {
            var returnAction = _actions.AddAction(ai.Owner, "ActionMalfAiReturnToCore");
            if (returnAction != null)
                shunted.ReturnAction = returnAction.Value;
        }

        _popup.PopupEntity(Loc.GetString("malfai-shunt-success"), popupTarget, ai);
        args.Handled = true;
    }

    private void OnReturnToCore(Entity<StationAiHeldComponent> ai, ref MalfAiReturnToCoreActionEvent args)
    {
        var popupTarget = GetAiEyeForPopup(ai.Owner) ?? ai.Owner;

        // Only Malf AI can return via this action.
        if (!HasComp<MalfunctioningAiComponent>(ai))
            return;

        // If the AI is currently hijacking a mech, delegate to hijack system instead.
        if (HasComp<MalfAiMechHijackComponent>(ai))
        {
            var hijackSys = EntityManager.System<MalfAiHijackMechSystem>();
            hijackSys.ReturnFromHijack(ai.Owner);
            args.Handled = true;
            return;
        }

        if (!TryComp<MalfAiShuntedComponent>(ai, out var shunted) || shunted.CoreHolder == null)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-no-core"), popupTarget, ai);
            return;
        }

        // Ensure we are currently in a container (e.g., in an APC).
        if (!_containers.TryGetContainingContainer((ai.Owner, null, null), out var currentContainer) || currentContainer is not ContainerSlot)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-not-shunted"), popupTarget, ai);
            return;
        }

        // Validate core holder: if missing or invalid, eject to floor with message.
        var coreHolder = shunted.CoreHolder.Value;
        if (Deleted(coreHolder) || !HasComp<StationAiCoreComponent>(coreHolder))
        {
            // Eject from current container and drop next to the current holder (e.g., APC)
            _containers.Remove(ai.Owner, currentContainer);
            _transform.DropNextTo(ai.Owner, currentContainer.Owner);

            // Cleanup: remove return action and clear recorded core holder
            if (shunted.ReturnAction != null)
            {
                _actions.RemoveAction(shunted.ReturnAction.Value);
                shunted.ReturnAction = null;
            }
            shunted.CoreHolder = null;

            _popup.PopupEntity("Core not found!", popupTarget, ai);
            args.Handled = true;
            return;
        }

        // Ensure the core still has/gets the correct container.
        var coreContainer = _containers.EnsureContainer<ContainerSlot>(coreHolder, StationAiHolderComponent.Container);

        // If the core slot is occupied, we cannot return.
        if (coreContainer.ContainedEntities.Count != 0)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-core-occupied"), popupTarget, ai);
            return;
        }

        // Move back into the core holder.
        var previousHolder = currentContainer.Owner;
        _containers.Remove(ai.Owner, currentContainer);
        _containers.Insert(ai.Owner, coreContainer);

        // Keep the AI marked as held.
        EnsureComp<StationAiHeldComponent>(ai);

        // If we had dynamically added a StationAiHolderComponent to an APC, clean it up when empty.
        if (HasComp<ApcComponent>(previousHolder))
        {
            var prevContainer = _containers.EnsureContainer<ContainerSlot>(previousHolder, StationAiHolderComponent.Container);
            if (prevContainer.ContainedEntities.Count == 0)
                RemCompDeferred<StationAiHolderComponent>(previousHolder);
        }

        // Close any open viewport UI on return as well (safety: this ensures a fresh open at core).
        if (TryComp<ActorComponent>(ai, out var actor) && actor.PlayerSession != null)
            RaiseNetworkEvent(new MalfAiViewportCloseEvent(), actor.PlayerSession);

        // Remove the return action after use.
        if (shunted.ReturnAction != null)
        {
            _actions.RemoveAction(shunted.ReturnAction.Value);
            shunted.ReturnAction = null;
        }

        _popup.PopupEntity(Loc.GetString("malfai-return-success"), popupTarget, ai);
        args.Handled = true;
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
