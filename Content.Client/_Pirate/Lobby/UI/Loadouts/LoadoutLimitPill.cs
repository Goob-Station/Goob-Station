using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._Pirate.Lobby.UI.Loadouts;

/// <summary>Selected/max badge for loadout group headers.</summary>
public sealed class LoadoutLimitPill : PanelContainer
{
    private readonly Label _label;

    public LoadoutLimitPill()
    {
        VerticalAlignment = VAlignment.Center;
        _label = new Label
        {
            StyleClasses = { "font-small" },
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0),
        };
        AddChild(_label);
    }

    public void SetCount(int selected, int max)
    {
        _label.Text = $"{selected}/{max}";
        var full = selected >= max && max > 0;
        var style = LoadoutStyles.RoundedBordered(full ? Color.FromHex("#2e7d32") : Color.FromHex("#42424f"), 0, 3);
        // Keep the pill compact without flattening the corners.
        style.ContentMarginTopOverride = 0;
        style.ContentMarginBottomOverride = 0;
        PanelOverride = style;
    }
}
