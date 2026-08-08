using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Feeds the Slashers fear meter.
/// </summary>
public sealed class SlasherFearAlertSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherFearComponent, GetGenericAlertCounterAmountEvent>(OnGetCounter);
        SubscribeLocalEvent<FearedComponent, GetGenericAlertCounterAmountEvent>(OnGetVictimCounter);
    }

    private void OnGetCounter(Entity<SlasherFearComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled || ent.Comp.Alert != args.Alert)
            return;

        args.Amount = (int) MathF.Round(ent.Comp.Meter);
    }

    private void OnGetVictimCounter(Entity<FearedComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled || ent.Comp.Alert != args.Alert)
            return;

        args.Amount = (int) MathF.Round(ent.Comp.Fear * 100f);
    }
}
