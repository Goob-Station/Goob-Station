using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Common.CCVar;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceJoinPromptUIController : UIController, IOnStateEntered<LobbyState>, IOnStateEntered<GameplayState>
{
    [Dependency] private readonly IClientNetManager _net = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly VoiceChatManager _voice = default!;

    private static readonly TimeSpan PromptDelay = TimeSpan.FromSeconds(3);

    private FancyWindow? _window;
    private TimeSpan? _promptAt;
    private bool _prompted;

    public override void Initialize()
    {
        base.Initialize();

        _net.Connected += OnConnected;
        _voice.WebConnectedChanged += OnWebConnectedChanged;
    }

    public void OnStateEntered(LobbyState state)
    {
        SchedulePrompt();
    }

    public void OnStateEntered(GameplayState state)
    {
        SchedulePrompt();
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_promptAt is not { } promptAt || _timing.RealTime < promptAt)
            return;

        _promptAt = null;
        if (_prompted ||
            _voice.WebConnected ||
            !_net.IsConnected ||
            !_cfg.GetCVar(GoobCVars.VoiceChatEnabled) ||
            !_cfg.GetCVar(GoobCVars.VoiceChatJoinPrompt))
        {
            return;
        }

        _prompted = true;
        OpenWindow();
    }

    private void SchedulePrompt()
    {
        if (!_prompted && _promptAt == null)
            _promptAt = _timing.RealTime + PromptDelay;
    }

    private void OnConnected(object? sender, NetChannelArgs args)
    {
        _prompted = false;
        _promptAt = null;
        CloseWindow();
    }

    private void OnWebConnectedChanged(bool connected)
    {
        if (connected)
            CloseWindow();
    }

    private void OpenWindow()
    {
        CloseWindow();

        var open = new Button
        {
            Text = Loc.GetString("voice-prompt-open"),
            StyleClasses = { StyleClass.ButtonOpenRight },
            HorizontalExpand = true,
        };
        var later = new Button
        {
            Text = Loc.GetString("voice-prompt-later"),
            StyleClasses = { StyleClass.ButtonOpenBoth },
            HorizontalExpand = true,
        };
        var never = new Button
        {
            Text = Loc.GetString("voice-prompt-never"),
            StyleClasses = { StyleClass.ButtonOpenLeft },
            HorizontalExpand = true,
        };

        var buttons = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(0, 8, 0, 0),
        };
        buttons.AddChild(open);
        buttons.AddChild(later);
        buttons.AddChild(never);

        var body = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8),
        };
        body.AddChild(new Label { Text = Loc.GetString("voice-prompt-text") });
        body.AddChild(buttons);

        var window = new FancyWindow
        {
            Title = Loc.GetString("voice-prompt-title"),
            MinWidth = 380,
        };
        window.ContentsContainer.AddChild(body);
        window.OnClose += () =>
        {
            if (_window == window)
                _window = null;
        };

        open.OnPressed += _ =>
        {
            _voice.RequestLink();
            window.Close();
        };
        later.OnPressed += _ => window.Close();
        never.OnPressed += _ =>
        {
            _cfg.SetCVar(GoobCVars.VoiceChatJoinPrompt, false);
            _cfg.SaveToFile();
            window.Close();
        };

        _window = window;
        window.OpenCentered();
    }

    private void CloseWindow()
    {
        _window?.Close();
        _window = null;
    }
}
