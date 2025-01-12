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

namespace Content.Shared.Vehicle.Clowncar;

/* TODO
 - Enter do after when entering the vehicle
 - Roll the dice action when emaged
 - Explode if someone that has drank more than 30u of irish car bomb enters the car
 - Spread space lube on damage with a prob of 33%
 - Repair with bananas
 - Sometimes the toggle cannon action repeats
 - Cannon fires weird in rotated grids
 - When shooting a second time the server crashes
 - You can buckle nonclowns as a third party
 - Player feedback like popups and chat messages for bumping, crashing, repairing, irish bomb, lubing, emag, squishing, dice roll, and all other features
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
        SubscribeLocalEvent<ClowncarComponent, EntGotInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ClowncarComponent, BuckledEvent>(OnBuckle);
        SubscribeLocalEvent<ClowncarComponent, UnbuckledEvent>(OnUnBuckle);
        SubscribeLocalEvent<ClowncarComponent, ClowncarFireModeActionEvent>(OnClowncarFireModeAction);
        SubscribeLocalEvent<ClowncarComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }

    /// <summary>
    /// Handles adding the "thank rider" action to passengers
    /// </summary>
    private void OnEntInserted(EntityUid uid, ClowncarComponent component, EntGotInsertedIntoContainerMessage args)
    {
        if (!TryComp<VehicleComponent>(uid, out var vehicle)
            || vehicle.Driver == null)
            return;
        /*
        component.ThankRiderAction = new()// TODO Action sustem needs rework
        {
            Icon = new SpriteSpecifier.Texture(new ("Objects/Fun/bikehorn.rsi/icon.png")),
            DisplayName = Loc.GetString("clowncar-action-name-thankrider"),
            Description = Loc.GetString("clowncar-action-desc-thankrider"),
            UseDelay = TimeSpan.FromSeconds(60),
            Event = new ThankRiderActionEvent()
        };*/

        _actionsSystem.AddAction(args.Entity, component.ThankRiderAction, uid);
    }

    /// <summary>
    /// Handles making the "toggle cannon" action available only when the car is emagged
    /// </summary>
    private void OnGotEmagged(EntityUid uid, ClowncarComponent component, ref GotEmaggedEvent args)
    {
       /* component.CannonAction = new()// TODO Action sustem needs rework
        {
            Icon = new SpriteSpecifier.Texture(new ("Objects/Weapons/Guns/Launchers/pirate_cannon.rsi/icon.png")),
            DisplayName = Loc.GetString("clowncar-action-name-firemode"),
            Description = Loc.GetString("clowncar-action-desc-firemode"),
            UseDelay = component.CannonSetupDelay,
            Event = new ClowncarFireModeActionEvent(),
        };

        args.Handled = true;*/
    }
    /// <summary>
    /// Handles preventing collision with the rider and
    /// adding/removing the "toggle cannon" action from the rider when available,
    /// also deactivates the cannon when the rider unbuckles
    /// </summary>
    private void OnBuckle(EntityUid uid, ClowncarComponent component, ref BuckledEvent args)
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
    }
    private void OnUnBuckle(EntityUid uid, ClowncarComponent component, ref UnbuckledEvent args)
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
    }

    /// <summary>
    /// Handles activating/deactivating the cannon when requested
    /// </summary>
    private void OnClowncarFireModeAction(EntityUid uid, ClowncarComponent component, ClowncarFireModeActionEvent args)
    {/*
        ToggleCannon(uid, component, args.Performer, component.CannonEntity == null);
        args.Handled = true;
        */
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
    }

    private void ToggleCannon(EntityUid uid, ClowncarComponent component, EntityUid user, bool activated)
    {/*
        var ourTransform = Transform(uid);
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
}

[Serializable, NetSerializable]
public sealed partial class ClownCarDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class ThankRiderActionEvent : InstantActionEvent
{
}

public sealed partial class ClowncarFireModeActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public enum ClowncarVisuals : byte
{
    FireModeEnabled
}
