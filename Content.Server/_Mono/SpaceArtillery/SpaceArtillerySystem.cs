using System.Numerics;
using Content.Server._Mono.FireControl;
using Content.Server._Mono.SpaceArtillery.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Mono.ShipGuns;
using Content.Shared._Mono.SpaceArtillery;
using Content.Shared.Camera;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Examine;
using Content.Shared.Power;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server._Mono.SpaceArtillery;

public sealed partial class SpaceArtillerySystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoilSystem = default!;
    [Dependency] private readonly FireControlSystem _fireControl = default!;

    private const float DISTANCE = 100;
    private const float BIG_DAMAGE = 1000;
    private const float BIG_DAMAGE_KICK = 35;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("SpaceArtillery");
        SubscribeLocalEvent<SpaceArtilleryComponent, AmmoShotEvent>(OnShotEvent);
        SubscribeLocalEvent<SpaceArtilleryComponent, PowerChangedEvent>(OnApcChanged);
        SubscribeLocalEvent<SpaceArtilleryComponent, OnEmptyGunShotEvent>(OnEmptyShotEvent);
        SubscribeLocalEvent<SpaceArtilleryComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<SpaceArtilleryComponent, ChargeChangedEvent>(OnBatteryChargeChanged);
        SubscribeLocalEvent<ShipWeaponProjectileComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<ShipGunClassComponent, ExaminedEvent>(OnExamined);
    }


    private void OnSignalReceived(EntityUid uid, SpaceArtilleryComponent component, ref SignalReceivedEvent args)
    {
        if (!TryComp<DeviceLinkSinkComponent>(uid, out var source))
            return;

        if (args.Port != component.SpaceArtilleryFirePort)
            OnMalfunction(uid, component);

        var hasBattery = TryComp<BatteryComponent>(uid, out var battery);

        if (!TryComp<ApcPowerReceiverComponent>(uid, out var apc) && !hasBattery)
            return;

        if (apc is { Powered: true } || battery?.CurrentCharge >= component.PowerUseActive)
            TryFireArtillery(uid, Transform(uid), component);
        else
            OnMalfunction(uid, component);
    }


    private void OnApcChanged(EntityUid uid, SpaceArtilleryComponent component, ref PowerChangedEvent args)
    {
        if (TryComp<BatterySelfRechargerComponent>(uid, out var batteryCharger))
        {
            if (args.Powered)
            {
                batteryCharger.AutoRecharge = true;
                batteryCharger.AutoRechargeRate = component.PowerChargeRate;
            }
            else
            {
                batteryCharger.AutoRecharge = true;
                batteryCharger.AutoRechargeRate = component.PowerUsePassive * -1;
            }
        }
    }


    private void OnBatteryChargeChanged(EntityUid uid, SpaceArtilleryComponent component, ref ChargeChangedEvent args)
    {
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcPowerReceiver) && TryComp<BatteryComponent>(uid, out var battery))
        {
            apcPowerReceiver.Load = battery.CurrentCharge >= battery.MaxCharge * 0.99 ? component.PowerUsePassive : component.PowerUsePassive + component.PowerChargeRate;
        }
    }

    private void TryFireArtillery(EntityUid uid, TransformComponent xform, SpaceArtilleryComponent component)
    {
        if (xform.GridUid == null && !xform.MapUid.HasValue)
        {
            return;
        }

        if (!_gun.TryGetGun(uid, out var gunUid, out var gun))
        {
            OnMalfunction(uid, component);
            return;
        }

        var worldPosX = _xform.GetWorldPosition(uid).X;
        var worldPosY = _xform.GetWorldPosition(uid).Y;
        var worldRot = _xform.GetWorldRotation(uid) + Math.PI;
        var targetSpot = new Vector2(worldPosX - DISTANCE * (float)Math.Sin(worldRot), worldPosY + DISTANCE * (float)Math.Cos(worldRot));

        // Create coordinates for the target and source positions
        var sourceCoordinates = xform.Coordinates;
        var targetCoordinates = new EntityCoordinates(xform.MapUid!.Value, targetSpot);

        // We need to set the ShootCoordinates for the gun component
        // This is important to ensure it uses the proper calculations in SharedGunSystem
        gun.ShootCoordinates = targetCoordinates;

        // Call AttemptShoot with the correct signature that includes target coordinates
        // This will eventually call GunSystem.Shoot which correctly handles grid velocity
        _gun.AttemptShoot(uid, gunUid, gun, targetCoordinates);
    }

    private void OnShotEvent(EntityUid uid, SpaceArtilleryComponent component, AmmoShotEvent args)
    {
        if (args.FiredProjectiles.Count == 0)
        {
            OnMalfunction(uid, component);
            return;
        }

        if (TryComp<BatteryComponent>(uid, out var battery))
        {
            _battery.UseCharge(uid, component.PowerUseActive, battery);
        }
    }

    private void OnEmptyShotEvent(EntityUid uid, SpaceArtilleryComponent component, OnEmptyGunShotEvent args)
    {
        OnMalfunction(uid, component);
    }

    private void OnMalfunction(EntityUid uid, SpaceArtilleryComponent component)
    {
    }

    private void OnProjectileHit(EntityUid uid, ShipWeaponProjectileComponent component, ProjectileHitEvent hitEvent)
    {
        var grid = Transform(hitEvent.Target).GridUid;
        if (grid == null)
            return;

        var players = Filter.Empty();
        players.AddInGrid((EntityUid)grid);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid playerEnt)
                continue;

            var vector = _xform.GetWorldPosition(uid) - _xform.GetWorldPosition(playerEnt);

            _recoilSystem.KickCamera(playerEnt, vector.Normalized() * (float)hitEvent.Damage.GetTotal() / BIG_DAMAGE * BIG_DAMAGE_KICK);
        }
    }

    private void OnExamined(EntityUid uid, ShipGunClassComponent component, ExaminedEvent args)
    {
        if (!TryComp<FireControllableComponent>(uid, out var controllable))
            return;
        if (!args.IsInDetailsRange)
            return;
        args.PushMarkup(
            Loc.GetString(
                "ship-gun-class-component-examine-detail",
                ("processingPower", _fireControl.GetProcessingPowerCost(uid, controllable))
            )
        );
    }
}
