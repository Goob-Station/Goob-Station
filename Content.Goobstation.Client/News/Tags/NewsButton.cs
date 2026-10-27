using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.News.Tags;

public sealed class NewsButton : IDocumentTag
{
    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        var style = args.GetValueOrDefault("Style") ?? "Plate";
        var button = new Button
        {
            Text = args.TryGetValue("Text", out var text) ? Loc.GetString(text) : string.Empty,
            ToggleMode = args.TryGetValue("Toggle", out var toggle) ? bool.TryParse(toggle, out var parsed) && parsed : style == "Tab",
            HorizontalAlignment = NewsTagArgs.HAlign(args, Control.HAlignment.Left),
            VerticalAlignment = Control.VAlignment.Center,
            Margin = NewsTagArgs.Margin(args),
        };

        switch (style)
        {
            case "Danger":
                button.AddStyleClass(HullStyleClasses.Danger);
                break;
            case "Tab":
                button.AddStyleClass(HullStyleClasses.Tab);
                break;
            case "Ready":
                button.AddStyleClass(HullStyleClasses.ReadyArt);
                button.ToggleMode = true;
                button.ToolTip = button.Text;
                button.Label.Visible = false;
                button.SetSize = new Vector2(128, 64);
                break;
            default:
                button.AddStyleClass(HullStyleClasses.Plate);
                break;
        }

        control = button;
        return true;
    }
}
