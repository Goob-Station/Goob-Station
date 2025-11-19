using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Alert.Components;

namespace Content.Goobstation.Client.Wraith;

public sealed class WraithPointsClientSystem : EntitySystem
{
    [Dependency] private readonly WraithPointsSystem _wraithPointsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithPointsComponent, GetGenericAlertCounterAmountEvent>(OnGenericCounterAlert);
    }

    private void OnGenericCounterAlert(Entity<WraithPointsComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled
            || ent.Comp.Alert != args.Alert)
            return;

        args.Amount = _wraithPointsSystem.GetCurrentWp(ent.Owner).Int();
    }
}
