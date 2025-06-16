using System.Collections.Generic;
using Content.Pirate.Shared.Aiming.Events;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Pirate.Shared.Aiming;

public sealed partial class SharedCanTakeAimSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityManager _entMan = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanTakeAimComponent, AfterInteractEvent>(OnWeaponTakeAim);
        SubscribeLocalEvent<CanTakeAimComponent, OnAimingTargetMoveEvent>(OnAimingTargetMove);
        SubscribeLocalEvent<CanTakeAimComponent, AmmoShotEvent>(OnAmmoShot);
    }
    private void OnAmmoShot(EntityUid uid, CanTakeAimComponent component, AmmoShotEvent args)
    {
        if (component.User != null)
        {
            var ev = new OnAimerShootingEvent(uid, component.User.Value);
            foreach (var entity in component.AimingAt.ToArray())
            {
                if (HasComp<OnSightComponent>(entity))
                {
                    RaiseLocalEvent(entity, ev);
                }
            }
            component.IsAiming = false;
            component.AimingAt.Clear();
            // Dirty(uid, component);
        }
        // TODO: Add damage multiplying
        // foreach (var entity in args.FiredProjectiles)
        // {
        // if (TryComp<ProjectileComponent>(entity, out var projectile))
        // {
        //     var deltaT = (_timing.CurFrame - component.AimStartFrame) * _timing.FramesPerSecondAvg;
        //     Logger.Debug($"Delta T: {deltaT}s");
        //     projectile.Damage *= component.MaxDamageMultiplier * (component.MaxAimTime / deltaT);

        //     Dirty(entity, projectile);
        // }
        // }
    }

    private void OnAimingTargetMove(EntityUid uid, CanTakeAimComponent component, OnAimingTargetMoveEvent args)
    {
        component.IsAiming = false;
        // Dirty(uid, component);
        if (!TryComp<GunComponent>(uid, out var gunComp))
            return;
        var targetCoords = _transform.ToCoordinates(_transform.GetMapCoordinates(args.Target));
        var gunCoords = _transform.ToCoordinates(_transform.GetMapCoordinates(uid));
        ;
        // if (_gun.CanShoot(gunComp))
        // {
        EntityUid? ammo = null;
        if (TryComp<ChamberMagazineAmmoProviderComponent>(uid, out var chamberComp))
        {

            if (chamberComp.BoltClosed != null && chamberComp.BoltClosed.Value == false)
            {
                _popup.PopupClient(Loc.GetString("gun-chamber-bolt-ammo"), component.User, PopupType.Medium);
                return;
            }
            ammo = _gun.GetChamberEntity(uid);
        }
        if (TryComp<RevolverAmmoProviderComponent>(uid, out var revolverComp))
        {
            if (revolverComp.Chambers[revolverComp.CurrentIndex] == false)
            {
                _popup.PopupClient(Loc.GetString("gun-chamber-bolt-ammo"), component.User, PopupType.Medium);
                return;
            }
            var fromCoords = _transform.ToCoordinates(_transform.GetMapCoordinates(uid));
            var takeAmmoEvent = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoords, component.User);
            RaiseLocalEvent(uid, takeAmmoEvent);

            if (takeAmmoEvent.Ammo.Count > 0)
            {
                ammo = takeAmmoEvent.Ammo[0].Entity;
            }
        }
        if (TryComp<BallisticAmmoProviderComponent>(uid, out var ballisticComp))
        {

            // Use TakeAmmo to safely get ammo
            var fromCoords = _transform.ToCoordinates(_transform.GetMapCoordinates(uid));
            var takeAmmoEvent = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoords, component.User);
            RaiseLocalEvent(uid, takeAmmoEvent);
            if (takeAmmoEvent.Ammo.Count > 0)
            {
                ammo = takeAmmoEvent.Ammo[0].Entity;
            }
        }
        if (TryComp<MagazineAmmoProviderComponent>(uid, out var magazineComp))
        {
            var magEntity = _gun.GetMagazineEntity(uid);
            if (magEntity == null)
                return;
            var fromCoords = _transform.ToCoordinates(_transform.GetMapCoordinates(uid));
            var takeAmmoEvent = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoords, component.User);
            RaiseLocalEvent(uid, takeAmmoEvent);
            ammo = takeAmmoEvent.Ammo[0].Entity;
        }
        if (ammo == null)
            return;
        _gun.Shoot(uid, gunComp, ammo.Value, gunCoords, targetCoords, out _, component.User);
        if (component.User == null)
            return;
        if (ballisticComp != null)
        {
            // For ballistic guns, use the ActivateInWorldEvent to trigger cycling
            if (ballisticComp.AutoCycle)
            {
                var activateEvent = new ActivateInWorldEvent(component.User.Value, uid, true);
                RaiseLocalEvent(uid, activateEvent);
            }
        }
        else if (chamberComp != null)
        {
            if (chamberComp.AutoCycle)
            {
                var useEvent = new UseInHandEvent(component.User.Value);
                RaiseLocalEvent(uid, useEvent);
            }
        }
        // }
        // _gun.ShootProjectile(ammo.Value, direction, _physics.GetMapLinearVelocity(uid, physComp), uid);
    }
    public void OnWeaponTakeAim(EntityUid uid, CanTakeAimComponent component, ref AfterInteractEvent args)
    {
        if (args.Target == null)
            return;
        if (args.Target == args.User)
            return;
        component.User = args.User;
        if (!args.CanReach)
        {
            _popup.PopupClient("Я не зможу попасти в ціль звідси...", args.User, PopupType.Medium);
            return;
        }
        if (!HasComp<MobMoverComponent>(args.Target))
            return;
        if (!TryComp<MetaDataComponent>(args.User, out var userMetaComp) || !TryComp<MetaDataComponent>(args.Target, out var targetMetaComp))
            return;
        if (component.IsAiming)
        {
            if (HasComp<OnSightComponent>(args.Target))
            {
                RemComp<OnSightComponent>(args.Target.Value);
                _popup.PopupPredicted($"{userMetaComp.EntityName} stopped aiming at {targetMetaComp.EntityName}.", args.Target.Value, args.Target.Value, PopupType.Large);
                component.IsAiming = false;
            }
            return;
        }

        if (userMetaComp == null || targetMetaComp == null)
            return;
        if (TryComp<MobStateComponent>(args.Target, out var targetMobState) && targetMobState.CurrentState != MobState.Alive)
            return;

        component.AimStartFrame = _timing.CurFrame;
        if (!component.AimingAt.Contains(args.Target.Value))
            component.AimingAt.Add(args.Target.Value);
        EnsureComponentOnTarget(args.Target.Value, uid, args.User);
        component.IsAiming = true;
        _popup.PopupPredicted($"{userMetaComp.EntityName} is aiming at {targetMetaComp.EntityName}!", args.Target.Value, args.Target.Value, PopupType.LargeCaution);

    }
    private void EnsureComponentOnTarget(EntityUid target, EntityUid uid, EntityUid userUid) // uid is uid of gun, not user!!!
    {
        EnsureComp<OnSightComponent>(target, out var onSigthComp);
        if (!onSigthComp.AimedAtWith.Contains(uid))
            onSigthComp.AimedAtWith.Add(uid);

        if (!onSigthComp.AimedAtBy.Contains(userUid))
            onSigthComp.AimedAtBy.Add(userUid);
        // Dirty(target, onSigthComp);
    }

}
