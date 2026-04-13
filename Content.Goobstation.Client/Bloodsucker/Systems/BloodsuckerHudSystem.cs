using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Shared.Alert.Components;

namespace Content.Goobstation.Client.Bloodsucker.Systems;

public sealed class BloodsuckerHudSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, GetGenericAlertCounterAmountEvent>(OnGetBloodCounter);
    }

    private void OnGetBloodCounter(Entity<BloodsuckerComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        // Blood alert
        if (args.Alert == ent.Comp.BloodAlert)
        {
            args.Amount = ent.Comp.CurrentBlood;
            //args.Handled = true;
            return;
        }

        // Rank alert
        if (args.Alert == ent.Comp.RankAlert)
        {
            args.Amount = ent.Comp.Rank;
            //args.Handled = true;
            return;
        }

        // Sol alert
        if (args.Alert == ent.Comp.SolAlert)
        {
            var cycle = GetDayNightCycle();
            if (cycle == null)
                return;

            args.Amount = (int) cycle.Value.Comp.TimeUntilCycle;
            //args.Handled = true;
        }
    }

    private Entity<BloodsuckerDayNightComponent>? GetDayNightCycle()
    {
        var query = EntityQueryEnumerator<BloodsuckerDayNightComponent>();
        if (query.MoveNext(out var uid, out var comp))
            return (uid, comp);
        return null;
    }
}
