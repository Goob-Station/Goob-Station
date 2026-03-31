using System.Globalization;
using Content.Goobstation.Shared.Sandevistan;
using Content.Shared.Alert.Components;
using Robust.Client.Player;

namespace Content.Goobstation.Client.Sandevistan;

public sealed class SandevistanAlertSystem : EntitySystem
{
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanUserComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);

        _loc.AddFunction(new CultureInfo("en-US"), "SANDE_THRESHOLD", FormatSandeThreshold);
    }

    private void OnGetCounterAmount(Entity<SandevistanUserComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled || ent.Comp.LoadAlert != args.Alert)
            return;

        args.Amount = (int) ent.Comp.CurrentLoad;
    }

    private ILocValue FormatSandeThreshold(LocArgs args)
    {
        var stateName = ((LocValueString) args.Args[0]).Value;

        if (_player.LocalEntity is { } player
            && TryComp<SandevistanUserComponent>(player, out var comp)
            && Enum.TryParse<SandevistanState>(stateName, true, out var state)
            && comp.Thresholds.TryGetValue(state, out var threshold))
            return new LocValueNumber(threshold.Double());

        return new LocValueString("?");
    }
}
