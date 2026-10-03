using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Mech.Systems;

public sealed class MechPowerDrainSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMechSystem _mech = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechPowerDrainComponent, MoveEvent>(OnMove);
    }

    private void OnMove(Entity<MechPowerDrainComponent> ent, ref MoveEvent args)
    {
        if (args.ParentChanged)
            return;

        if (!TryComp<MechComponent>(ent, out var mech) || mech.PilotSlot.ContainedEntity == null)
            return;

        var distance = (args.NewPosition.Position - args.OldPosition.Position).Length();
        if (distance <= 0f || distance > 2f)
            return;

        ent.Comp.TileDistance += distance;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MechPowerDrainComponent, MechComponent>();
        while (query.MoveNext(out var uid, out var drain, out var mech))
        {
            if (curTime < drain.NextUpdate)
                continue;

            drain.NextUpdate = curTime + drain.UpdateInterval;
            Dirty(uid, drain);

            var distance = drain.TileDistance;
            drain.TileDistance = 0f;

            if (mech.Broken || mech.PilotSlot.ContainedEntity == null || mech.Energy <= 0)
                continue;

            if (mech.OnChargingStation)
                continue;

            var cost = drain.IdleDrain.Float() * (float) drain.UpdateInterval.TotalSeconds
                       + drain.PerTileMovementDrain.Float() * distance;

            if (cost <= 0f)
                continue;

            var delta = FixedPoint2.Min(FixedPoint2.New(cost), mech.Energy);
            if (!_mech.TryChangeEnergy(uid, -delta, mech))
                continue;

            if (mech.Energy <= 0)
                _popup.PopupEntity(Loc.GetString("mech-power-depleted-popup", ("mech", uid)), uid, PopupType.MediumCaution);
        }
    }
}
