// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ichaie <167008606+Ichaie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 MortalBaguette <169563638+MortalBaguette@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Poips <Hanakohashbrown@gmail.com>
// SPDX-FileCopyrightText: 2025 PuroSlavKing <103608145+PuroSlavKing@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 blobadoodle <me@bloba.dev>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 kamkoi <poiiiple1@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 tetra <169831122+Foralemes@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Lobby.UI;
using Content.Client.Message;
using Content.Goobstation.Common.CCVar;
using Content.Shared._RMC14.LinkAccount;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;
using static Robust.Client.UserInterface.Controls.LineEdit;
using static Robust.Client.UserInterface.Controls.TabContainer;

namespace Content.Client._RMC14.LinkAccount;

public sealed class LinkAccountUIController : UIController, IOnSystemChanged<LinkAccountSystem>
{
    [Dependency] private readonly IClipboardManager _clipboard = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;

    private LinkAccountWindow? _window;
    private PatronPerksWindow? _patronPerksWindow;
    private TimeSpan _disableUntil;

    private Guid _code;

    public override void Initialize()
    {
        _linkAccount.CodeReceived += OnCode;
        _linkAccount.Updated += OnUpdated;
    }

    private void OnCode(Guid code)
    {
        _code = code;

        if (_window == null)
            return;

        _window.CopyButton.Disabled = false;
    }

    private void OnUpdated()
    {
        if (UIManager.ActiveScreen is not LobbyGui gui)
            return;

        gui.CharacterPreview.PatronPerks.Visible = _linkAccount.CanViewPatronPerks();
    }

    private void OnLobbyMessageReceived(SharedRMCDisplayLobbyMessageEvent message)
    {
        if (UIManager.ActiveScreen is not LobbyGui gui)
            return;

        var user = FormattedMessage.EscapeText(message.User);
        var msg = FormattedMessage.EscapeText(message.Message);
        gui.LobbyMessageLabel.SetMarkupPermissive($"[font size=20]Lobby message by: {user}\n{msg}[/font]");
    }

    public void ToggleWindow()
    {
        if (_window == null)
        {
            _window = new LinkAccountWindow();
            _window.OnClose += () => _window = null;
            _window.Label.SetMarkupPermissive($"{Loc.GetString("rmc-ui-link-discord-account-text")}");
            if (_linkAccount.Linked)
                _window.Label.SetMarkupPermissive($"{Loc.GetString("rmc-ui-link-discord-account-already-linked")}\n\n{Loc.GetString("rmc-ui-link-discord-account-text")}");

            _window.CopyButton.OnPressed += _ =>
            {
                _clipboard.SetText(_code.ToString());
                _window.CopyButton.Text = Loc.GetString("rmc-ui-link-discord-account-copied");
                _window.CopyButton.Disabled = true;
                _disableUntil = _timing.RealTime.Add(TimeSpan.FromSeconds(3));
            };

            var messageLink = _config.GetCVar(GoobCVars.RMCDiscordAccountLinkingMessageLink);
            if (string.IsNullOrEmpty(messageLink))
            {
                _window.LinkButton.Visible = false;
                _window.CopyButton.RemoveStyleClass("OpenRight");
            }
            else
            {
                _window.LinkButton.Visible = true;
                _window.LinkButton.OnPressed += _ => _uriOpener.OpenUri(messageLink);
                _window.CopyButton.AddStyleClass("OpenRight");
            }

            _window.OpenCentered();

            if (_code == default)
                _window.CopyButton.Disabled = true;

            _net.ClientSendMessage(new LinkAccountRequestMsg());
            return;
        }

        _window.Close();
        _window = null;
    }

    public void TogglePatronPerksWindow()
    {
        if (_patronPerksWindow == null)
        {
            _patronPerksWindow = new PatronPerksWindow();
            _patronPerksWindow.OnClose += () => _patronPerksWindow = null;

            var tier = _linkAccount.Tier;
            SetTabTitle(_patronPerksWindow.LobbyMessageTab, Loc.GetString("rmc-ui-lobby-message"));
            SetTabVisible(_patronPerksWindow.LobbyMessageTab, tier is { LobbyMessage: true });
            _patronPerksWindow.LobbyMessage.OnTextEntered += ChangeLobbyMessage;
            _patronPerksWindow.LobbyMessage.OnFocusExit += ChangeLobbyMessage;

            if (_linkAccount.LobbyMessage?.Message is { } lobbyMessage)
                _patronPerksWindow.LobbyMessage.Text = lobbyMessage;

            SetTabTitle(_patronPerksWindow.ShoutoutTab, Loc.GetString("rmc-ui-shoutout"));
            SetTabVisible(_patronPerksWindow.ShoutoutTab, tier is { RoundEndShoutout: true });
            _patronPerksWindow.NTShoutout.OnTextEntered += ChangeNTShoutout;
            _patronPerksWindow.NTShoutout.OnFocusExit += ChangeNTShoutout;

            if (_linkAccount.RoundEndShoutout?.NT is { } ntShoutout)
                _patronPerksWindow.NTShoutout.Text = ntShoutout;

            SetTabTitle(_patronPerksWindow.GhostColorTab, Loc.GetString("rmc-ui-ghost-color"));
            SetTabVisible(_patronPerksWindow.GhostColorTab, tier is { GhostColor: true });
            _patronPerksWindow.GhostColorSliders.Color = _linkAccount.GhostColor ?? Color.White;
            _patronPerksWindow.GhostColorSliders.OnColorChanged += OnGhostColorChanged;
            _patronPerksWindow.GhostColorClearButton.OnPressed += OnGhostColorClear;
            _patronPerksWindow.GhostColorSaveButton.OnPressed += OnGhostColorSave;

            UpdateExamples();

            for (var i = 0; i < _patronPerksWindow.Tabs.ChildCount; i++)
            {
                var child = _patronPerksWindow.Tabs.GetChild(i);
                if (!child.GetValue(TabVisibleProperty))
                    continue;

                _patronPerksWindow.Tabs.CurrentTab = i;
                break;
            }

            _patronPerksWindow.OpenCentered();
            return;
        }

        _patronPerksWindow.Close();
        _patronPerksWindow = null;
    }

    private void ChangeLobbyMessage(LineEditEventArgs args)
    {
        var text = args.Text;
        if (text.Length > SharedRMCLobbyMessage.CharacterLimit)
        {
            text = text[..SharedRMCLobbyMessage.CharacterLimit];
            _patronPerksWindow?.LobbyMessage.SetText(text, false);
        }

        _net.ClientSendMessage(new RMCChangeLobbyMessageMsg { Text = text });
    }

    private void ChangeNTShoutout(LineEditEventArgs args)
    {
        var text = args.Text;
        if (text.Length > SharedRMCRoundEndShoutouts.CharacterLimit)
        {
            text = text[..SharedRMCRoundEndShoutouts.CharacterLimit];
            _patronPerksWindow?.LobbyMessage.SetText(text, false);
        }

        _net.ClientSendMessage(new RMCChangeNTShoutoutMsg { Name = text });
        UpdateExamples();
    }

    private void OnGhostColorChanged(Color color)
    {
        if (_patronPerksWindow is not { IsOpen: true })
            return;

        _patronPerksWindow.GhostColorSaveButton.Disabled = false;
    }

    private void OnGhostColorClear(ButtonEventArgs args)
    {
        if (_patronPerksWindow is not { IsOpen: true })
            return;

        _patronPerksWindow.GhostColorSliders.Color = Color.White;
        _net.ClientSendMessage(new RMCClearGhostColorMsg());
    }

    private void OnGhostColorSave(ButtonEventArgs args)
    {
        if (_patronPerksWindow is not { IsOpen: true })
            return;

        _net.ClientSendMessage(new RMCChangeGhostColorMsg { Color = _patronPerksWindow.GhostColorSliders.Color });
    }

    private void UpdateExamples()
    {
        if (_patronPerksWindow == null)
            return;

        var nt = _patronPerksWindow.NTShoutout.Text.Trim();
        _patronPerksWindow.NTShoutoutExample.SetMarkupPermissive(string.IsNullOrWhiteSpace(nt)
            ? " "
            : $"{Loc.GetString("rmc-ui-shoutout-example")} {Loc.GetString("rmc-ui-shoutout-nt", ("name", nt))}");
    }

    public void OnSystemLoaded(LinkAccountSystem system)
    {
        system.LobbyMessageReceived += OnLobbyMessageReceived;
    }

    public void OnSystemUnloaded(LinkAccountSystem system)
    {
        system.LobbyMessageReceived -= OnLobbyMessageReceived;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        if (_window == null)
            return;

        var time = _timing.RealTime;
        if (_disableUntil != default && time > _disableUntil)
        {
            _disableUntil = default;
            _window.CopyButton.Text = Loc.GetString("rmc-ui-link-discord-account-copy");
            _window.CopyButton.Disabled = false;
        }
    }
}