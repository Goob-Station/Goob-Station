using System.Globalization;
using Content.Goobstation.Shared.Sandevistan;
using Content.Shared.Alert.Components;
using Robust.Client.Player;

namespace Content.Goobstation.Client.Sandevistan;

public sealed class SandevistanAlertSystem : EntitySystem
{
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private static readonly string[] SeverityColors = ["yellow", "orange", "red", "darkred"];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanUserComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);

        _loc.AddFunction(new CultureInfo("en-US"), "SANDE_LOAD_DESC", FormatSandeLoadDesc);
    }

    private void OnGetCounterAmount(Entity<SandevistanUserComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled || ent.Comp.LoadAlert != args.Alert)
            return;

        args.Amount = (int) ent.Comp.CurrentLoad;
    }

    private ILocValue FormatSandeLoadDesc(LocArgs args)
    {
        if (!(_player.LocalEntity is { } player && TryComp<SandevistanUserComponent>(player, out var sandeusercomp)))
            return new LocValueString("");

        var thresholds = sandeusercomp.Thresholds;
        var lines = new List<string>(thresholds.Count);
        var i = 0;

        foreach (var (state, threshold) in thresholds)
        {
            var colorIndex = thresholds.Count == 1
                ? 0
                : (int) Math.Round((double) i / (thresholds.Count - 1) * (SeverityColors.Length - 1));

            var locKey = $"sandevistan-effect-desc-{state.ToString().ToLower()}";
            var desc = _loc.TryGetString(locKey, out var localized) ? localized : state.ToString();

            lines.Add($"At [color={SeverityColors[colorIndex]}]{threshold}[/color] {desc}");
            i++;
        }

        return new LocValueString(string.Join("\n", lines));
    }
}
