using System.Numerics;
using Content.Client._RMC14.LinkAccount;
using Content.Client.Administration.UI.CustomControls;
using Content.Client.Changelog;
using Content.Client.Credits;
using Content.Client.Info;
using Content.Client.Message;
using Content.Client.UserInterface.Systems.Chat.Controls;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Client.Voting.UI;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Content.Shared.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class LobbyGui
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IUriOpener _uri = default!;

    private readonly List<(CVarDef<string> CVar, Button Button)> _linkButtons = new();

    private void InitializeGoob()
    {
        CreateMenu(LinksButton, BuildLinks());
        CreateMenu(MenuButton, BuildMenu());

        NewsTabButton.OnPressed += _ => SelectTab(false);
        ChatTabButton.OnPressed += _ => SelectTab(true);
        Chat.ChatInput.Input.OnFocusEnter += _ => SelectTab(true);
        SelectTab(true);
        RestyleChat();

        SetRoundTimer(string.Empty, string.Empty);
    }

    private void RestyleChat()
    {
        Chat.ChatInput.RemoveStyleClass(ChatInputBox.StyleClassChatPanel);
        Chat.ChatInput.AddStyleClass(HullStyleClasses.ChatInput);

        foreach (var row in Chat.ChatInput.Children)
        {
            foreach (var child in row.Children)
            {
                if (child is not BaseButton button)
                    continue;

                button.RemoveStyleClass(ChannelSelectorItemButton.StyleClassChatSelectorOptionButton);
                button.RemoveStyleClass(ChatInputBox.StyleClassChatFilterOptionButton);
                button.AddStyleClass(HullStyleClasses.Plate);
            }
        }
    }

    public void SelectTab(bool chat)
    {
        NewsContainer.Visible = !chat;
        Chat.Visible = chat;
        NewsTabButton.Pressed = !chat;
        ChatTabButton.Pressed = chat;
    }

    public void UpdateReadyArt()
    {
        var art = ReadyButton.ToggleMode;
        ReadyButton.Label.Visible = !art;
        ReadyButton.ToolTip = art ? ReadyButton.Text : null;

        ReadyButton.RemoveStyleClass(art ? HullStyleClasses.Ready : HullStyleClasses.ReadyArt);
        ReadyButton.AddStyleClass(art ? HullStyleClasses.ReadyArt : HullStyleClasses.Ready);
        ReadyButton.SetSize = art ? new Vector2(128, 64) : new Vector2(160, 38);
    }

    public void SetNewsUnread(bool unread)
    {
        NewsLamp.Visible = unread;
    }

    public void SetRoundTimer(string caption, string value)
    {
        StartTimeCaption.Text = caption;
        StartTime.Text = value;
    }

    public void SetLobbyMessage(string markup)
    {
        LobbyMessageLabel.SetMarkupPermissive(markup);
        LobbyMessagePanel.Visible = true;
    }

    public void SetRoundInfo(string map, string mode, int roundId, int players, int ready)
    {
        MapValue.Text = FormattedMessage.RemoveMarkupPermissive(map);
        ModeValue.Text = FormattedMessage.RemoveMarkupPermissive(mode);
        RoundValue.Text = Loc.GetString("ui-lobby-fact-round-value", ("roundId", roundId));
        ReadyValue.Text = Loc.GetString("ui-lobby-fact-ready-value", ("ready", ready), ("players", players));
    }

    private List<BaseButton> BuildLinks()
    {
        var items = new List<BaseButton>();

        var rules = new Button { Text = Loc.GetString("server-info-rules-button") };
        rules.OnPressed += _ => new RulesAndInfoWindow().Open();
        items.Add(rules);

        var guidebook = new Button { Text = Loc.GetString("server-info-guidebook-button") };
        guidebook.OnPressed += _ => UserInterfaceManager.GetUIController<GuidebookUIController>().ToggleGuidebook();
        items.Add(guidebook);

        var changelog = new ChangelogButton();
        changelog.OnPressed += _ => UserInterfaceManager.GetUIController<ChangelogUIController>().ToggleWindow();
        items.Add(changelog);

        AddLink("server-info-discord-button", CCVars.InfoLinksDiscord);
        AddLink("server-info-website-button", CCVars.InfoLinksWebsite);
        AddLink("server-info-wiki-button", CCVars.InfoLinksWiki);
        AddLink("server-info-forum-button", CCVars.InfoLinksForum);
        AddLink("server-info-telegram-button", CCVars.InfoLinksTelegram);
        AddLink("rmc-ui-patreon", CCVars.InfoLinksPatreon);

        var linkAccount = new Button { Text = Loc.GetString("rmc-ui-link-discord-account") };
        linkAccount.OnPressed += _ => UserInterfaceManager.GetUIController<LinkAccountUIController>().ToggleWindow();
        items.Add(linkAccount);

        AddLink("server-info-report-button", CCVars.InfoLinksBugReport);

        var credits = new Button { Text = Loc.GetString("server-info-credits-button") };
        credits.OnPressed += _ => new CreditsWindow().Open();
        items.Add(credits);

        return items;

        void AddLink(string loc, CVarDef<string> cvar)
        {
            var button = new Button { Text = Loc.GetString(loc) };
            button.OnPressed += _ => _uri.OpenUri(_cfg.GetCVar(cvar));
            _linkButtons.Add((cvar, button));
            items.Add(button);
        }
    }

    private List<BaseButton> BuildMenu()
    {
        var options = new Button { Text = Loc.GetString("ui-lobby-options-button") };
        options.OnPressed += _ => UserInterfaceManager.GetUIController<OptionsUIController>().ToggleWindow();

        var achievements = new CommandButton
        {
            Command = "achievements",
            Text = Loc.GetString("achievements-button"),
        };

        var leave = new Button { Text = Loc.GetString("ui-lobby-leave-button") };
        leave.OnPressed += _ => _consoleHost.ExecuteCommand("disconnect");

        return new List<BaseButton> { options, new VoteCallMenuButton(), achievements, leave };
    }

    private void CreateMenu(BaseButton anchor, List<BaseButton> items)
    {
        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 1,
            MinWidth = 180,
        };
        var panel = new PanelContainer { StyleClasses = { HullStyleClasses.MenuPopup } };
        panel.AddChild(box);

        var popup = new Popup();
        popup.AddChild(panel);
        popup.OnPopupHide += () => popup.Orphan();

        foreach (var item in items)
        {
            item.StyleClasses.Add(HullStyleClasses.MenuItem);
            item.OnPressed += _ => popup.Close();
            box.AddChild(item);
        }

        anchor.OnPressed += _ => OpenMenu(anchor, popup, panel);
    }

    private void OpenMenu(Control anchor, Popup popup, Control content)
    {
        foreach (var (cvar, button) in _linkButtons)
        {
            button.Visible = _cfg.GetCVar(cvar) != string.Empty;
        }

        UserInterfaceManager.ModalRoot.AddChild(popup);
        content.Measure(Vector2Helpers.Infinity);

        var size = content.DesiredSize;
        var position = anchor.GlobalPosition + new Vector2(anchor.Width - size.X, anchor.Height + 2);
        popup.Open(UIBox2.FromDimensions(position, size));
    }
}
