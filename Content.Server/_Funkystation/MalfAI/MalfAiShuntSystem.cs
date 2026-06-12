// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Destructible;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Destructible;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Containers;
using Content.Shared.Database;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles Malf AI shunting of the AI brain to APCs, and returning to the core.
/// The APC is turned into a proper AI holder (StationAiHolder + the AiHeld component
/// registry), so the AI keeps its overlay, radio and speech, and an intellicard can
/// download the AI from the APC exactly like from the core.
/// </summary>
public sealed class MalfAiShuntSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly Robust.Shared.Prototypes.IPrototypeManager _proto = default!;
    [Dependency] private readonly ExplosionSystem _explosions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    private const string ApcShuntKit = "MalfAiApcShuntKit";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiShuntToApcActionEvent>(OnShuntToApc);
        SubscribeLocalEvent<MalfAiShuntedComponent, MalfAiReturnToCoreActionEvent>(OnReturnToCore);
        SubscribeLocalEvent<MalfAiShuntedComponent, EntGotRemovedFromContainerMessage>(OnShuntedRemoved);
        SubscribeNetworkEvent<MalfAiShuntConfirmResponseEvent>(OnShuntConfirmed);
        SubscribeLocalEvent<StationAiHolderComponent, DestructionEventArgs>(OnHolderDestroyed);
        SubscribeLocalEvent<StationAiHolderComponent, EntityTerminatingEvent>(OnHolderTerminating);
    }

    private void OnShuntToApc(Entity<MalfAiMarkerComponent> ent, ref MalfAiShuntToApcActionEvent args)
    {
        if (args.Handled)
            return;

        var ai = ent.Owner;

        if (!HasComp<ApcComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("malfai-shunt-not-apc"), ai, ai);
            return;
        }

        // Shunting while the doomsday protocol runs would abort it: ask the player first.
        if (TryComp<MalfAiDoomsdayComponent>(ai, out var doom) && doom.Active &&
            TryComp<ActorComponent>(ai, out var doomActor))
        {
            RaiseNetworkEvent(new MalfAiShuntConfirmRequestEvent(GetNetEntity(args.Target)), doomActor.PlayerSession);
            args.Handled = true;
            return;
        }

        TryShuntToApc(ai, args.Target);
        args.Handled = true;
    }

    private void OnShuntConfirmed(MalfAiShuntConfirmResponseEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } ai || !HasComp<MalfAiMarkerComponent>(ai))
            return;

        var target = GetEntity(ev.Apc);
        if (!Exists(target) || !HasComp<ApcComponent>(target))
            return;

        TryShuntToApc(ai, target);
    }

    private void TryShuntToApc(EntityUid ai, EntityUid target)
    {

        // Ensure the AI is currently inside a holder (core or APC) and get that holder.
        if (!_containers.TryGetContainingContainer((ai, null, null), out var currentContainer) || currentContainer is not ContainerSlot)
        {
            _popup.PopupEntity(Loc.GetString("malfai-shunt-no-holder"), ai, ai);
            return;
        }

        var currentHolder = currentContainer.Owner;

        // The kit turns the APC into a proper AI holder: intellicard slot, AiHeld registry, AI vision.
        var destContainer = _containers.EnsureContainer<ContainerSlot>(target, StationAiHolderComponent.Container);

        // Transparent like the core's mind slot, so the held AI can still target the world.
        destContainer.ShowContents = true;

        if (destContainer.ContainedEntities.Count != 0)
        {
            _popup.PopupEntity(Loc.GetString("malfai-shunt-apc-occupied"), ai, ai);
            return;
        }

        // Apply the holder kit (locked holder slot, AiHeld registry, AI vision) from YAML,
        // so the slot is configured exactly like the core without runtime field writes.
        var kit = _proto.Index<EntityPrototype>(ApcShuntKit);
        EntityManager.AddComponents(target, kit.Components, removeExisting: false);

        // Remember the original core holder for return if we have not recorded it yet.
        var shunted = EnsureComp<MalfAiShuntedComponent>(ai);
        shunted.CoreHolder ??= currentHolder;

        // Move the AI brain to the APC. The flag keeps OnShuntedRemoved from wiping
        // the shunt state (original core, return action) during an APC-to-APC transfer.
        shunted.Transferring = true;
        _containers.Remove(ai, currentContainer);
        _containers.Insert(ai, destContainer);
        shunted.Transferring = false;

        // Show the blue (emag-style) screen on the inhabited APC.
        if (TryComp<ApcComponent>(target, out var apcComp))
            apcComp.NeedStateUpdate = true;

        // Leaving the core reset the eye (drawFov true, target cleared): view from the APC without a FOV cone.
        if (TryComp<EyeComponent>(ai, out var eyeComp))
        {
            _eye.SetDrawFov(ai, false, eyeComp);
            _eye.SetTarget(ai, null, eyeComp);
        }

        // Close any open viewport: its anchor may be out of range now.
        if (TryComp<MalfAiViewportComponent>(ai, out var viewport))
        {
            viewport.Selected = null;
            viewport.IsWindowOpen = false;
        }

        if (TryComp<ActorComponent>(ai, out var actor))
            RaiseNetworkEvent(new MalfAiViewportCloseEvent(), actor.PlayerSession);

        // Grant Return to Core action while shunted.
        if (shunted.ReturnAction == null)
        {
            EntityUid? returnAction = null;
            _actions.AddAction(ai, ref returnAction, "ActionMalfAiReturnToCore");
            shunted.ReturnAction = returnAction;
        }

        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"Malf AI {ToPrettyString(ai)} shunted into APC {ToPrettyString(target)}");

        _popup.PopupEntity(Loc.GetString("malfai-shunt-success"), ai, ai);
    }

    private void OnReturnToCore(Entity<MalfAiShuntedComponent> ent, ref MalfAiReturnToCoreActionEvent args)
    {
        if (args.Handled)
            return;

        var ai = ent.Owner;
        var shunted = ent.Comp;

        if (shunted.CoreHolder == null)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-no-core"), ai, ai);
            return;
        }

        // Ensure we are currently in a container (e.g., in an APC).
        if (!_containers.TryGetContainingContainer((ai, null, null), out var currentContainer) || currentContainer is not ContainerSlot)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-not-shunted"), ai, ai);
            return;
        }

        // If the core was destroyed, the AI is trapped in the APC: no eject, just inform.
        var coreHolder = shunted.CoreHolder.Value;
        if (Deleted(coreHolder) || !HasComp<StationAiCoreComponent>(coreHolder))
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-no-core"), ai, ai);
            args.Handled = true;
            return;
        }

        // A broken core cannot host the AI: stay in the APC until someone repairs it.
        if (TryComp<DestructibleComponent>(coreHolder, out var destructible) && destructible.IsBroken)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-core-broken"), ai, ai);
            args.Handled = true;
            return;
        }

        // Ensure the core still has/gets the correct container.
        var coreContainer = _containers.EnsureContainer<ContainerSlot>(coreHolder, StationAiHolderComponent.Container);

        if (coreContainer.ContainedEntities.Count != 0)
        {
            _popup.PopupEntity(Loc.GetString("malfai-return-core-occupied"), ai, ai);
            return;
        }

        // Move back into the core holder. APC-side cleanup happens in OnShuntedRemoved.
        _containers.Remove(ai, currentContainer);
        _containers.Insert(ai, coreContainer);

        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"Malf AI {ToPrettyString(ai)} returned to core {ToPrettyString(coreHolder)}");

        args.Handled = true;
    }

    /// <summary>
    /// When an APC holding a shunted AI is destroyed, try an emergency transfer back to the
    /// core; if the core is gone or occupied, the AI dies in a small explosion.
    /// </summary>
    private void OnHolderDestroyed(Entity<StationAiHolderComponent> ent, ref DestructionEventArgs args)
    {
        EvacuateOrPerish(ent);
    }

    /// <summary>
    /// Catch-all for every other way the APC can disappear (deconstruction, entity replacement,
    /// admin deletion...): the brain would silently die with it otherwise.
    /// </summary>
    private void OnHolderTerminating(Entity<StationAiHolderComponent> ent, ref EntityTerminatingEvent args)
    {
        EvacuateOrPerish(ent);
    }

    private void EvacuateOrPerish(Entity<StationAiHolderComponent> ent)
    {
        if (!HasComp<ApcComponent>(ent.Owner))
            return;

        if (!_containers.TryGetContainer(ent.Owner, StationAiHolderComponent.Container, out var container) ||
            container.ContainedEntities.Count == 0)
            return;

        var ai = container.ContainedEntities[0];
        if (!TryComp<MalfAiShuntedComponent>(ai, out var shunted))
            return;

        // Emergency transfer back to the original core if it still exists, works and is free.
        if (shunted.CoreHolder is { } core && !Deleted(core) && HasComp<StationAiCoreComponent>(core) &&
            (!TryComp<DestructibleComponent>(core, out var destructible) || !destructible.IsBroken))
        {
            var coreContainer = _containers.EnsureContainer<ContainerSlot>(core, StationAiHolderComponent.Container);
            if (coreContainer.ContainedEntities.Count == 0)
            {
                _containers.Remove(ai, container);
                _containers.Insert(ai, coreContainer);

                _popup.PopupEntity(Loc.GetString("malfai-shunt-emergency-return"), ai, ai, PopupType.LargeCaution);

                _adminLogger.Add(LogType.Action, LogImpact.Medium,
                    $"Malf AI {ToPrettyString(ai)} emergency-returned to core after APC destruction");
                return;
            }
        }

        // Nowhere to go: the AI perishes with the APC.
        PerishInApc(ai, ent.Owner);
    }

    /// <summary>
    /// The shunted AI dies inside its APC host: small explosion, public popup, death.
    /// </summary>
    private void PerishInApc(EntityUid ai, EntityUid apc)
    {
        // Note: if the mind is projected into a borg/mech, the ChangeMobState below
        // pulls it back via the death handlers in the remote-control/hijack systems.
        var coords = _xforms.GetMapCoordinates(apc);
        _explosions.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
            totalIntensity: 5f, slope: 1f, maxTileIntensity: 3f, cause: null, maxTileBreak: 1);

        _popup.PopupCoordinates(Loc.GetString("malfai-shunt-ai-destroyed"), Transform(apc).Coordinates, PopupType.LargeCaution);

        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"Malf AI {ToPrettyString(ai)} was destroyed inside APC {ToPrettyString(apc)}");

        _mobState.ChangeMobState(ai, MobState.Dead);
        QueueDel(ai);
    }

    /// <summary>
    /// Cleans up the APC and the shunt state whenever the shunted brain leaves an APC holder,
    /// whether through the return action or an intellicard download.
    /// </summary>
    private void OnShuntedRemoved(Entity<MalfAiShuntedComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        var previousHolder = args.Container.Owner;
        if (!HasComp<ApcComponent>(previousHolder))
            return;

        var shunted = ent.Comp;

        // Strip the holder kit we added to the APC.
        var kit = _proto.Index<EntityPrototype>(ApcShuntKit);
        EntityManager.RemoveComponents(previousHolder, kit.Components);

        // Back to the normal screen.
        if (TryComp<ApcComponent>(previousHolder, out var apcComp))
            apcComp.NeedStateUpdate = true;

        // APC-to-APC transfer: keep the original core and the return action.
        if (shunted.Transferring)
            return;

        if (shunted.ReturnAction != null)
        {
            _actions.RemoveAction(ent.Owner, shunted.ReturnAction.Value);
            shunted.ReturnAction = null;
        }

        RemCompDeferred<MalfAiShuntedComponent>(ent.Owner);
    }
}
