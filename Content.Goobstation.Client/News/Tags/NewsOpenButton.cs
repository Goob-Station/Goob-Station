using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Content.Goobstation.Client.News.UI;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.News.Tags;

public sealed partial class NewsOpenButton : Button, IDocumentTag
{
    private string? _entry;

    public NewsOpenButton()
    {
        HorizontalAlignment = HAlignment.Left;
        AddStyleClass(HullStyleClasses.Plate);
        OnPressed += _ =>
        {
            if (_entry != null)
                UserInterfaceManager.GetUIController<NewsUIController>().Open(_entry);
        };
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!args.TryGetValue("Entry", out var entry))
        {
            Logger.GetSawmill("news").Error("NewsOpenButton needs an Entry");
            return false;
        }

        _entry = entry;
        Text = args.TryGetValue("Text", out var text) ? Loc.GetString(text) : entry;

        control = this;
        return true;
    }
}
