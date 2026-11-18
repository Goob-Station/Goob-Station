using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.News.Tags;

public sealed class NewsFact : IDocumentTag
{
    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        var plate = new PanelContainer
        {
            HorizontalExpand = true,
            Margin = NewsTagArgs.Margin(args),
            StyleClasses = { HullStyleClasses.Fact },
        };

        var row = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 6 };
        row.AddChild(new Label
        {
            Text = args.TryGetValue("Key", out var key) ? Loc.GetString(key) : string.Empty,
            StyleClasses = { HullStyleClasses.FactKey },
        });
        row.AddChild(new Label
        {
            Text = args.TryGetValue("Value", out var value) ? Loc.GetString(value) : string.Empty,
            HorizontalExpand = true,
            ClipText = true,
            StyleClasses = { HullStyleClasses.FactValue },
        });
        plate.AddChild(row);

        control = plate;
        return true;
    }
}
