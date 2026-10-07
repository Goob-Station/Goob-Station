using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Examine;
using Content.Shared.Mech.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class MechPowerCageSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCageDecayComponent, ContainerGettingInsertedAttemptEvent>(OnCageInsertAttempt);
        SubscribeLocalEvent<PowerCageDecayComponent, ExaminedEvent>(OnCageExamined);
    }

    private void OnCageInsertAttempt(Entity<PowerCageDecayComponent> cage, ref ContainerGettingInsertedAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<ChargerComponent>(args.Container.Owner, out var charger)
            || args.Container.ID != charger.SlotId)
            return;

        args.Cancel();
    }

    private void OnCageExamined(Entity<PowerCageDecayComponent> ent, ref ExaminedEvent args) =>
        args.PushMarkup(Loc.GetString("mech-power-cage-examine", ("seconds", (int) ent.Comp.DecayTime.TotalSeconds)));

    public bool IsInMech(EntityUid cage)
    {
        return _container.TryGetContainingContainer((cage, null, null), out var container)
               && TryComp<MechComponent>(container.Owner, out var mech)
               && container.ID == mech.BatterySlotId;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<PowerCageDecayComponent, BatteryComponent>();
        while (query.MoveNext(out var uid, out var decay, out var battery))
        {
            if (curTime < decay.NextUpdate)
                continue;

            decay.NextUpdate = curTime + decay.UpdateInterval;
            Dirty(uid, decay);

            var charge = _battery.GetCharge((uid, battery));
            if (charge <= 0 || IsInMech(uid))
                continue;

            var loss = battery.MaxCharge * (float) (decay.UpdateInterval.TotalSeconds / decay.DecayTime.TotalSeconds);
            _battery.SetCharge((uid, battery), MathF.Max(0f, charge - loss));
        }
    }
}
