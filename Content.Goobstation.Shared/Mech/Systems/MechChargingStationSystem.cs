using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Shared.Mech.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Content.Shared.Mech.EntitySystems;

namespace Content.Goobstation.Shared.Mech.Systems;

/// <summary>
/// Charges any mechs that are in the same tile as the charging station.
/// </summary>
public sealed partial class MechChargingStationSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedMechSystem _mech = default!;
    [Dependency] private SharedPowerReceiverSystem _receiver = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechChargingStationComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MechChargingStationComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnExamined(Entity<MechChargingStationComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(MechChargingStationComponent)))
        {
            args.PushMarkup(Loc.GetString("mech-charging-station-examine-rate", ("chargeRate", (int) ent.Comp.APCMechChargeRate)));

            var xform = Transform(ent);
            if (!xform.Anchored || !_receiver.IsPowered(ent.Owner))
            {
                args.PushMarkup(Loc.GetString("mech-charging-station-examine-unpowered"));
                return;
            }

            var any = false;
            foreach (var mech in GetParkedMechs(ent.Comp, xform))
            {
                any = true;
                var percent = mech.Comp.MaxEnergy > 0 ? (int) ((mech.Comp.Energy / mech.Comp.MaxEnergy).Float() * 100) : 0;
                args.PushMarkup(Loc.GetString("mech-charging-station-examine-parked", ("mech", mech.Owner), ("percent", percent)));
            }

            if (!any)
                args.PushMarkup(Loc.GetString("mech-charging-station-examine-idle"));
        }
    }

    private void OnShutdown(Entity<MechChargingStationComponent> ent, ref ComponentShutdown args)
    {
        foreach (var mech in ent.Comp.ParkedMechs)
            SetOnStation(mech, false);

        ent.Comp.ParkedMechs.Clear();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MechChargingStationComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var station, out var xform))
        {
            if (curTime < station.NextUpdate)
                continue;

            station.NextUpdate = curTime + station.UpdateInterval;
            Dirty(uid, station);

            var charging = false;
            var parked = new HashSet<EntityUid>();
            if (xform.Anchored && _receiver.IsPowered(uid))
            {
                var amount = station.APCMechChargeRate * (float) station.UpdateInterval.TotalSeconds;
                foreach (var mech in GetParkedMechs(station, xform))
                {
                    parked.Add(mech);
                    SetOnStation(mech, true);

                    if (TryCharge(mech, amount))
                        charging = true;
                }
            }

            foreach (var left in station.ParkedMechs)
                if (!parked.Contains(left))
                    SetOnStation(left, false);

            station.ParkedMechs = parked;
            SetCharging((uid, station), charging);
        }
    }

    private void SetOnStation(EntityUid mech, bool onStation)
    {
        if (!TryComp<MechComponent>(mech, out var mechComp) || mechComp.OnChargingStation == onStation)
            return;

        mechComp.OnChargingStation = onStation;
        Dirty(mech, mechComp);
    }

    private bool TryCharge(Entity<MechComponent> mech, float amount)
    {
        if (mech.Comp.Broken || mech.Comp.BatterySlot.ContainedEntity == null)
            return false;

        var missing = mech.Comp.MaxEnergy - mech.Comp.Energy;
        if (missing <= 0)
            return false;

        var delta = FixedPoint2.Min(FixedPoint2.New(amount), missing);
        return _mech.TryChangeEnergy(mech, delta, mech.Comp);
    }

    private IEnumerable<Entity<MechComponent>> GetParkedMechs(MechChargingStationComponent station, TransformComponent xform)
    {
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            yield break;

        var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);

        foreach (var mech in _lookup.GetEntitiesInRange<MechComponent>(xform.Coordinates, station.ChargeRangeLimit))
        {
            var mechXform = Transform(mech);
            if (mechXform.GridUid != gridUid)
                continue;

            if (_map.TileIndicesFor(gridUid, grid, mechXform.Coordinates) != tile)
                continue;

            yield return mech;
        }
    }

    private void SetCharging(Entity<MechChargingStationComponent> ent, bool charging)
    {
        if (ent.Comp.IsCharging == charging)
            return;

        ent.Comp.IsCharging = charging;
        Dirty(ent);

        _receiver.SetLoad(ent.Owner, charging ? ent.Comp.APCMechChargeRate : ent.Comp.APCIdleDraw);
        _appearance.SetData(ent.Owner, MechChargingStationVisuals.Charging, charging);
        _ambient.SetAmbience(ent.Owner, charging);
        _light.SetEnabled(ent.Owner, charging);
    }
}
