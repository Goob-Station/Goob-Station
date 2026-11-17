using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.News.Tags;

public sealed class NewsPanel : IDocumentTag
{
    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        var plate = new StyleBoxFlat
        {
            BackgroundColor = HullPanel.PlateColor,
            BorderColor = HullPanel.EdgeColor,
            BorderThickness = new Thickness(1),
        };
        plate.SetContentMarginOverride(StyleBox.Margin.Horizontal, 26);
        plate.SetContentMarginOverride(StyleBox.Margin.Vertical, 12);

        control = new HullPanel
        {
            PanelOverride = plate,
            HorizontalAlignment = NewsTagArgs.HAlign(args, Control.HAlignment.Stretch),
            Margin = NewsTagArgs.Margin(args),
        };
        return true;
    }
}
