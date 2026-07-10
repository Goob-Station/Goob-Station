using Content.Shared._Pirate.RemoteDrone;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Trigger.Components;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._Pirate.FpvDrone;

public abstract class SharedFpvDroneSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLinkSystem = default!;
    [Dependency] private readonly SharedMoverController _moverController = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly RemoteDroneSystem _droneControllerSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FpvDroneComponent, MapInitEvent>(OnFpvMapInit);
        SubscribeLocalEvent<FpvDroneComponent, SignalReceivedEvent>(OnFpvSignalReceived);

        SubscribeLocalEvent<FpvDroneComponent, RemoteDroneLinkedEvent>(OnFpvLinked);
        SubscribeLocalEvent<FpvDroneControllerComponent, RemoteDroneUnlinkedEvent>(OnFpvUnlinked);
        SubscribeLocalEvent<FpvDroneComponent, PowerCellChangedEvent>(OnFpvCellChanged);

        SubscribeLocalEvent<FpvDroneComponent, PowerCellSlotEmptyEvent>(OnFpvPowerCellEmpty);
        SubscribeLocalEvent<FpvDroneComponent, GettingPickedUpAttemptEvent>(OnFpvAttemptPickup);

        SubscribeLocalEvent<FpvDroneComponent, RemoteDroneAttemptControlEvent>(OnFpvAttemptControl);
        SubscribeLocalEvent<FpvDroneComponent, RemoteDroneControlStartedEvent>(OnFpvControlStarted);
        SubscribeLocalEvent<FpvDroneComponent, RemoteDroneControlEndedEvent>(OnFpvControlEnded);
        SubscribeLocalEvent<FpvDroneControllerComponent, RemoteDroneControlEndedEvent>(OnFpvControllerControlEnded);
        SubscribeLocalEvent<FpvDroneComponent, ComponentShutdown>(OnFpvShutdown);
        SubscribeAllEvent<FpvDroneDropPayloadRequest>(OnDropPayloadRequest);

        CommandBinds.Builder
            .BindBefore(ContentKeyFunctions.Drop, new PointerInputCmdHandler(HandlePayloadDropInput, outsidePrediction: true), new[] { typeof(SharedHandsSystem) })
            .Register<SharedFpvDroneSystem>();
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<SharedFpvDroneSystem>();
        base.Shutdown();
    }

    private void OnFpvMapInit(Entity<FpvDroneComponent> entity, ref MapInitEvent args)
    {
        _deviceLinkSystem.EnsureSinkPorts(entity.Owner, entity.Comp.DropStoragePort);
    }

    private void OnFpvSignalReceived(Entity<FpvDroneComponent> entity, ref SignalReceivedEvent args)
    {
        if (args.Port != entity.Comp.DropStoragePort.ToString())
            return;

        TryDropPayload(entity);
    }

    private bool TryDropPayload(Entity<FpvDroneComponent> entity)
    {
        if (TerminatingOrDeleted(entity.Owner))
            return false;

        if (!_containerSystem.TryGetContainer(entity.Owner, entity.Comp.EmptiedContainerId, out var container))
            return false;

        if (container.Count == 0)
            return false;

        var payload = container.ContainedEntities[0];
        if (!TryActivatePayload(entity.Owner, payload))
            return false;

        if (TerminatingOrDeleted(payload))
            return false;

        if (container.Contains(payload) &&
            !_containerSystem.Remove(payload, container, force: true))
        {
            return false;
        }

        _popupSystem.PopupEntity(Loc.GetString("fpv-drone-payload-dropped", ("name", Identity.Name(entity.Owner, EntityManager))), entity.Owner, PopupType.MediumCaution);
        return true;
    }

    private bool TryActivatePayload(EntityUid user, EntityUid payload)
    {
        if (TerminatingOrDeleted(user) || TerminatingOrDeleted(payload))
            return false;

        var requiresActivation =
            HasComp<TriggerOnUseComponent>(payload) ||
            HasComp<TriggerOnActivateComponent>(payload);

        if (_interactionSystem.UseInHandInteraction(user, payload, checkCanUse: false, checkCanInteract: false))
            return PayloadActivationSucceeded(payload, requiresActivation);

        if (TerminatingOrDeleted(payload))
            return false;

        var activated = _interactionSystem.InteractionActivate(
            user,
            payload,
            checkCanInteract: false,
            checkAccess: false,
            complexInteractions: true);

        return activated
            ? PayloadActivationSucceeded(payload, requiresActivation)
            : !requiresActivation;
    }

    private bool PayloadActivationSucceeded(EntityUid payload, bool requiresActivation)
    {
        if (!requiresActivation)
            return true;

        if (TerminatingOrDeleted(payload))
            return false;

        if (HasComp<TimerTriggerComponent>(payload) &&
            !HasComp<ActiveTimerTriggerComponent>(payload))
        {
            return false;
        }

        return true;
    }

    private bool HandlePayloadDropInput(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (args.State != BoundKeyState.Down ||
            args.Session?.AttachedEntity is not { } user ||
            !TryComp<RelayInputMoverComponent>(user, out var relay) ||
            relay.RelayEntity == EntityUid.Invalid ||
            TerminatingOrDeleted(relay.RelayEntity) ||
            !HasComp<FpvDroneComponent>(relay.RelayEntity))
        {
            return false;
        }

        RaisePredictiveEvent(new FpvDroneDropPayloadRequest());
        return true;
    }

    private void OnDropPayloadRequest(FpvDroneDropPayloadRequest msg, EntitySessionEventArgs args)
    {
        if (!_netManager.IsServer ||
            args.SenderSession.AttachedEntity is not { } user ||
            TerminatingOrDeleted(user) ||
            !TryComp<RelayInputMoverComponent>(user, out var relay) ||
            relay.RelayEntity == EntityUid.Invalid ||
            TerminatingOrDeleted(relay.RelayEntity) ||
            !TryComp<FpvDroneComponent>(relay.RelayEntity, out var fpvComponent) ||
            !_droneControllerSystem.ResolveDroneAndController(relay.RelayEntity, out _, out var controllerEntity) ||
            !controllerEntity.Value.Comp.Controlling ||
            controllerEntity.Value.Comp.UserUid != user)
        {
            return;
        }

        TryDropPayload((relay.RelayEntity, fpvComponent));
    }

    private void OnFpvLinked(Entity<FpvDroneComponent> entity, ref RemoteDroneLinkedEvent args)
    {
        var fpvControllerComponent = EntityManager.ComponentFactory.GetComponent<FpvDroneControllerComponent>();
        fpvControllerComponent.HasSufficientCharge = _powerCellSystem.HasDrawCharge(entity.Owner);
        AddComp(args.ControllerEntity, fpvControllerComponent);
    }

    private void OnFpvUnlinked(Entity<FpvDroneControllerComponent> entity, ref RemoteDroneUnlinkedEvent args)
    {
        RemComp<FpvDroneControllerComponent>(entity);
    }

    private void OnFpvCellChanged(Entity<FpvDroneComponent> entity, ref PowerCellChangedEvent args)
    {
        if (args.Ejected)
            TryUpdateFpvChargeState(entity, false);
        else
            TryUpdateFpvChargeState(entity, _powerCellSystem.HasDrawCharge(entity.Owner));
    }

    private void TryUpdateFpvChargeState(EntityUid droneUid, bool hasSufficientCharge)
    {
        if (TerminatingOrDeleted(droneUid) ||
            !_droneControllerSystem.ResolveDroneAndController(droneUid, out _, out var controllerEntity) ||
            TerminatingOrDeleted(controllerEntity.Value.Owner))
            return;

        if (!TryComp<FpvDroneControllerComponent>(controllerEntity, out var fpvControllerComponent))
            return;

        fpvControllerComponent.HasSufficientCharge = hasSufficientCharge;
        Dirty(controllerEntity.Value.Owner, fpvControllerComponent);
    }

    private void OnFpvPowerCellEmpty(Entity<FpvDroneComponent> entity, ref PowerCellSlotEmptyEvent args)
    {
        if (!_droneControllerSystem.ResolveDroneAndController(entity.Owner, out _, out var controllerEntity) ||
            !controllerEntity.Value.Comp.Controlling)
            return;

        _droneControllerSystem.TryStopControlling(controllerEntity.Value);
    }

    private void OnFpvAttemptPickup(Entity<FpvDroneComponent> entity, ref GettingPickedUpAttemptEvent args)
    {
        if (!_droneControllerSystem.ResolveDroneAndController(entity.Owner, out _, out var controllerEntity) ||
            !controllerEntity.Value.Comp.Controlling)
            return;

        // Only cancel if the drone is currently being controlled.
        args.Cancel();
    }

    private void OnFpvAttemptControl(Entity<FpvDroneComponent> entity, ref RemoteDroneAttemptControlEvent args)
    {
        if (!TryComp<FpvDroneControllerComponent>(args.ControllerEntity, out var fpvControllerComponent))
        {
            args.Cancelled = true;
            return;
        }

        args.Cancelled |= !fpvControllerComponent.HasSufficientCharge;
    }

    private void OnFpvControlStarted(Entity<FpvDroneComponent> entity, ref RemoteDroneControlStartedEvent args)
    {
        if (!TryComp<PhysicsComponent>(entity, out var physicsComponent))
        {
            DebugTools.Assert($"Tried to handle RemoteDroneControlStartedEvent for FpvDroneComponent on an entity {ToPrettyString(entity.Owner)} without PhysicsComponent.");
            Log.Error($"Tried to handle RemoteDroneControlStartedEvent for FpvDroneComponent on an entity {ToPrettyString(entity.Owner)} without PhysicsComponent.");
            return;
        }

        // if the drone is being held in any hand then try to drop it
        if (_containerSystem.TryGetContainingContainer(entity.Owner, out var container) &&
            TryComp<HandsComponent>(container.Owner, out var handsComponent))
        {
            foreach (var handId in _handsSystem.EnumerateHands((container.Owner, handsComponent)))
            {
                var heldItem = _handsSystem.GetHeldItem((container.Owner, handsComponent), handId);
                if (heldItem != entity.Owner)
                    continue;

                _handsSystem.TryDrop(container.Owner, handId, checkActionBlocker: false);
            }
        }

        _physicsSystem.SetBodyStatus(entity.Owner, physicsComponent, BodyStatus.InAir);
        _moverController.SetRelay(args.ControllerEntity.Comp.UserUid!.Value, entity.Owner);

        if (_netManager.IsServer)
            entity.Comp.AudioUid ??= _audioSystem.PlayPvs(entity.Comp.AudioSpecifier, entity.Owner)?.Entity;

        if (TryComp<FlyBySoundComponent>(entity.Owner, out var flyBySoundComponent))
        {
            flyBySoundComponent.Prob = entity.Comp.FlybySoundProbability;
            Dirty(entity.Owner, flyBySoundComponent);
        }

        _powerCellSystem.SetDrawEnabled(entity.Owner, true);
        if (TryComp<PowerCellSlotComponent>(entity.Owner, out var powerCellSlotComponent))
            _itemSlotsSystem.SetLock(entity.Owner, powerCellSlotComponent.CellSlotId, true);

        _appearanceSystem.SetData(entity.Owner, FpvDroneVisuals.Active, true);
        OnDroneDisabled(args.ControllerEntity.Owner);
        UpdateFpvSurveillance(entity);
    }

    // Does nothing on client
    protected virtual void UpdateFpvSurveillance(Entity<FpvDroneComponent> entity) { }

    // Does nothing on client
    protected virtual void OnDroneDisabled(EntityUid uid) { }

    private void OnFpvControlEnded(Entity<FpvDroneComponent> entity, ref RemoteDroneControlEndedEvent args)
    {
        CleanupDroneFlightEffects(entity);

        if (TerminatingOrDeleted(entity.Owner))
            return;

        if (!TryComp<PhysicsComponent>(entity, out var physicsComponent))
        {
            DebugTools.Assert($"Tried to handle RemoteDroneControlEndedEvent for FpvDroneComponent on an entity {ToPrettyString(entity.Owner)} without PhysicsComponent.");
            Log.Error($"Tried to handle RemoteDroneControlEndedEvent for FpvDroneComponent on an entity {ToPrettyString(entity.Owner)} without PhysicsComponent.");
            return;
        }

        _physicsSystem.SetBodyStatus(entity.Owner, physicsComponent, BodyStatus.OnGround);

        RemCompDeferred<MovementRelayTargetComponent>(entity.Owner);

        if (TryComp<FlyBySoundComponent>(entity.Owner, out var flyBySoundComponent))
        {
            flyBySoundComponent.Prob = 0f;
            Dirty(entity.Owner, flyBySoundComponent);
        }

        _powerCellSystem.SetDrawEnabled(entity.Owner, false);
        if (TryComp<PowerCellSlotComponent>(entity.Owner, out var powerCellSlotComponent))
            _itemSlotsSystem.SetLock(entity.Owner, powerCellSlotComponent.CellSlotId, false);

        _appearanceSystem.SetData(entity.Owner, FpvDroneVisuals.Active, false);
    }

    private void OnFpvControllerControlEnded(Entity<FpvDroneControllerComponent> entity, ref RemoteDroneControlEndedEvent args)
    {
        CleanUpControllerControl(args.ControllerEntity);
        // Pirate: disconnect the controller monitor when flight ends; the source passed the drone uid here.
        OnDroneDisabled(args.ControllerEntity.Owner);
    }

    private void OnFpvShutdown(Entity<FpvDroneComponent> entity, ref ComponentShutdown args)
    {
        CleanupDroneFlightEffects(entity);

        if (TryComp<RemoteDroneComponent>(entity.Owner, out var remoteDrone) &&
            remoteDrone.LinkedControllerUid is { } controller)
        {
            OnDroneDisabled(controller);
        }
    }

    private void CleanUpControllerControl(Entity<RemoteDroneControllerComponent> controllerEntity)
    {
        if (controllerEntity.Comp.UserUid is { } userUid &&
            !Deleted(userUid))
            RemCompDeferred<RelayInputMoverComponent>(userUid);
    }

    private void CleanupDroneFlightEffects(Entity<FpvDroneComponent> entity)
    {
        QueueDel(entity.Comp.AudioUid);
        entity.Comp.AudioUid = null;
    }
}
