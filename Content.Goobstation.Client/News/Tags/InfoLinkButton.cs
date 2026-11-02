using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Content.Shared.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.News.Tags;

public sealed partial class InfoLinkButton : Button, IDocumentTag
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IUriOpener _uri = default!;

    private CVarDef<string>? _link;

    public InfoLinkButton()
    {
        IoCManager.InjectDependencies(this);
        HorizontalAlignment = HAlignment.Left;
        AddStyleClass(HullStyleClasses.Plate);
        OnPressed += _ => Open();
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!args.TryGetValue("Link", out var link) || GetLink(link) is not { } cvar)
        {
            Logger.GetSawmill("news").Error($"InfoLinkButton has an unknown Link: {link}");
            return false;
        }

        _link = cvar;
        Text = args.TryGetValue("Text", out var text) ? Loc.GetString(text) : link;
        Visible = _cfg.GetCVar(cvar) != string.Empty;

        control = this;
        return true;
    }

    private void Open()
    {
        if (_link == null)
            return;

        var url = _cfg.GetCVar(_link);
        if (url != string.Empty)
            _uri.OpenUri(url);
    }

    private static CVarDef<string>? GetLink(string? link)
    {
        return link switch
        {
            "Discord" => CCVars.InfoLinksDiscord,
            "Forum" => CCVars.InfoLinksForum,
            "Github" => CCVars.InfoLinksGithub,
            "Website" => CCVars.InfoLinksWebsite,
            "Wiki" => CCVars.InfoLinksWiki,
            "Patreon" => CCVars.InfoLinksPatreon,
            "BugReport" => CCVars.InfoLinksBugReport,
            "Appeal" => CCVars.InfoLinksAppeal,
            "Telegram" => CCVars.InfoLinksTelegram,
            _ => null,
        };
    }
}
