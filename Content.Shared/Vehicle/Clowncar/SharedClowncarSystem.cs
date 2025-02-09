using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Climbing.Components;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Vehicles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Content.Shared.DragDrop;
using Content.Shared.Emag.Components;

namespace Content.Shared.Vehicle.Clowncar;

/* TODO
 - Enter do after when entering the vehicle         //Done
 - Roll the dice action when emaged //not sure what to do whit this one
 - Explode if someone that has drank more than 30u of irish car bomb enters the car //done
 - Spread space lube on damage with a prob of 33% - //Done
 - Repair with bananas                              //Done
 - You can buckle nonclowns as a third party        //Done

 - Player feedback like popups  //
    and chat messages
    for bumping,
    crashing,
    repairing,
    irish bomb,
    lubing,
    emag,
    squishing,
    dice roll,
    and all other features

 - add a use of thank counter                       //Done

 no canon for now: coming in -vertion 2- one week away
    - Sometimes the toggle cannon action repeats
    - Cannon fires weird in rotated grids
    - When shooting a second time the server crashes
 */
public abstract partial class SharedClowncarSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;

    [Dependency] protected readonly SharedAppearanceSystem AppearanceSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClowncarComponent, GotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<ClowncarComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ClowncarComponent, StrappedEvent>(OnBuckle);
        SubscribeLocalEvent<ClowncarComponent, UnstrappedEvent>(OnUnBuckle);
        SubscribeLocalEvent<ClowncarComponent, ClowncarFireModeActionEvent>(OnClowncarFireModeAction);
        SubscribeLocalEvent<ClowncarComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }
    /// <summary>
    /// Handles adding the "thank rider" action to passengers
    /// </summary>
    private void OnEntInserted(EntityUid uid, ClowncarComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.Container)
            return;

        if (!TryComp<VehicleComponent>(uid, out var _))
            return;

        _actionsSystem.AddAction(args.Entity, component.ThankRiderAction, uid);
    }
    /// <summary>
    /// Handles making the "toggle cannon" action available only when the car is emagged
    /// </summary>
    private void OnGotEmagged(EntityUid uid, ClowncarComponent component, ref GotEmaggedEvent args)
    {
        EnsureComp<EmaggedComponent>(uid);

        if (!TryComp<VehicleComponent>(uid, out var vehicle)
            || vehicle.Driver == null)
            return;

        _actionsSystem.AddAction(vehicle.Driver.Value, component.CanonModeAction, uid);
        args.Handled = true;
        args.Repeatable = false;
    }
    /// <summary>
    /// Handles preventing collision with the rider and
    /// adding/removing the "toggle cannon" action from the rider when available,
    /// also deactivates the cannon when the rider unbuckles
    /// </summary>
    private void OnBuckle(EntityUid uid, ClowncarComponent component, ref StrappedEvent args)
    {/*
        var user = args.Buckle.Owner;
        if (args.Buckling)
            EnsureComp<PreventCollideComponent>(uid).Uid = user;
        switch (args.Buckling)
        {
            case true when component.CannonAction != null:
                component.CannonAction.Toggled = component.CannonEntity != null;
                _actionsSystem.AddAction(user, component.CannonAction, uid);
                break;
            case false when component.CannonAction != null:
            {
                _actionsSystem.RemoveAction(user, component.CannonAction);
                if (component.CannonEntity != null)
                    ToggleCannon(uid, component, args.BuckledEntity, false);
                break;
            }
        }*/
        if (HasComp<EmaggedComponent>(uid))
            _actionsSystem.AddAction(args.Buckle.Owner, component.CanonModeAction, uid);

        component.ThankCounter = 0;
    }
    private void OnUnBuckle(EntityUid uid, ClowncarComponent component, ref UnstrappedEvent args)
    {/*
        var user = args.BuckledEntity;
        if (args.Buckling)
            EnsureComp<PreventCollideComponent>(uid).Uid = user;
        switch (args.Buckling)
        {
            case true when component.CannonAction != null:
                component.CannonAction.Toggled = component.CannonEntity != null;
                _actionsSystem.AddAction(user, component.CannonAction, uid);
                break;
            case false when component.CannonAction != null:
            {
                _actionsSystem.RemoveAction(user, component.CannonAction);
                if (component.CannonEntity != null)
                    ToggleCannon(uid, component, args.BuckledEntity, false);
                break;
            }
        }*/
        if (HasComp<EmaggedComponent>(uid)){
            foreach (var action in _actionsSystem.GetActions(args.Buckle.Owner))
            {
                if (Name(action.Id) == component.CanonModeAction)
                    _actionsSystem.RemoveAction(action.Id);
            }
        }
        if (component.CannonEntity != null)
            ToggleCannon(uid, component, args.Buckle.Owner, true);
    }

    private void ToggleCannon(EntityUid uid, ClowncarComponent component, EntityUid user, bool activated)
    {
        /*var ourTransform = Transform(uid);
        var sound = activated ? component.CannonActivateSound : component.CannonDeactivateSound;
        _audioSystem.PlayPredicted(sound, ourTransform.Coordinates, uid);

        AppearanceSystem.SetData(uid, ClowncarVisuals.FireModeEnabled, activated);

        switch (activated)
        {
            case true when component.CannonEntity == null:
                if (!ourTransform.Anchored)
                    _transformSystem.AnchorEntity(uid, ourTransform);

                component.CannonEntity = Spawn(component.CannonPrototype, ourTransform.Coordinates);
                if (TryComp<HandsComponent>(user, out var handsComp)
                    && _handsSystem.TryGetEmptyHand(user, out var hand, handsComp))
                {
                    _handsSystem.TryPickup(user, component.CannonEntity.Value, hand, handsComp: handsComp);
                    _handsSystem.SetActiveHand(user, hand, handsComp);
                   // Comp<ContainerAmmoProviderComponent>(component.CannonEntity.Value).ProviderUid = uid;//needs some checking
                    _combatSystem.SetInCombatMode(user, true);
                }
                break;

            case false when component.CannonEntity != null:
                Del(component.CannonEntity.Value);
                component.CannonEntity = null;
                if (ourTransform.Anchored)
                    _transformSystem.Unanchor(uid, ourTransform);
                _combatSystem.SetInCombatMode(user, false);
                break;
    }*/
    }
    /// <summary>
    /// Handles making people knock down each other when fired
    /// </summary>
    private void OnEntRemoved(EntityUid uid, ClowncarComponent component, EntRemovedFromContainerMessage args)
    {
        /*_stunSystem.TryKnockdown(args.Entity, component.ShootingParalyzeTime, true);
        var paralyzeComp = EnsureComp<ParalyzeOnCollideComponent>(args.Entity);
        paralyzeComp.ParalyzeTime = component.ShootingParalyzeTime;
        paralyzeComp.CollidableEntities = new EntityWhitelist
        {
            Entities = new List<EntityUid> { uid }, //only use of whitelist entities?
            Components = new[]
            {
                _factory.GetComponentName(typeof(ItemComponent)),
                _factory.GetComponentName(typeof(ClimbableComponent)),
            }
        };*/

        if (args.Container.ID != component.Container)
            return;

        foreach (var ( actionId, comp ) in _actionsSystem.GetActions(args.Entity))
        {
            if (!TryComp(actionId, out MetaDataComponent? metaData))
                continue;
            if (metaData.EntityPrototype != null && metaData.EntityPrototype == component.ThankRiderAction)
                _actionsSystem.RemoveAction(actionId);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class ClownCarDoAfterEvent : SimpleDoAfterEvent { }
[Serializable, NetSerializable]
public sealed partial class ClownCarEnterDriverSeatDoAfterEvent : SimpleDoAfterEvent { }
[Serializable, NetSerializable]
public sealed partial class ClownCarOpenTrunkDoAfterEvent : SimpleDoAfterEvent { }
public sealed partial class ThankRiderActionEvent : InstantActionEvent { }
public sealed partial class ClowncarFireModeActionEvent : InstantActionEvent { }
public sealed partial class QuietBackThereActionEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public enum ClowncarVisuals : byte
{
    FireModeEnabled
}
