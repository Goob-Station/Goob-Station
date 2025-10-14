using Content.Client.Popups;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;

namespace Content.Goobstation.Client.Wraith;

public sealed class WraithPointsClientSystem : EntitySystem
{
    [Dependency] private readonly WraithPointsSystem _wraithPointsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithPointsComponent, GetGenericAlertCounterAmountEvent>(OnGenericCounterAlert);
        SubscribeLocalEvent<WraithPointsComponent, ClickAlertEvent>(OnClickAlert);
    }

    private void OnGenericCounterAlert(Entity<WraithPointsComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled
            || ent.Comp.Alert != args.Alert)
            return;

        args.Amount = _wraithPointsSystem.GetCurrentWp(ent.Owner).Int();
    }

    private void OnClickAlert(Entity<WraithPointsComponent> ent, ref ClickAlertEvent args)
    {
        if (args.Type != ent.Comp.Alert)
            return;

        if (!TryComp<PassiveWraithPointsComponent>(ent.Owner, out var passiveWraithPointsComponent))
            return; // popup something here

        // popup wp gen here
        _popupSystem.PopupClient("your shit", ent.Owner, ent.Owner);
    }
}
