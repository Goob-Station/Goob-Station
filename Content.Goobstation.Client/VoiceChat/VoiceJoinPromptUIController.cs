using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Goobstation.Client.VoiceChat.UI;
using Content.Goobstation.Common.CCVar;
using Robust.Client.UserInterface.Controllers;
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

    private TimeSpan? _promptAt;
    private bool _prompted;

    public override void Initialize()
    {
        base.Initialize();

        _net.Connected += OnConnected;
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
        UIManager.GetUIController<VoiceChatGuideUIController>().Close();
    }

    private void OpenWindow()
    {
        UIManager.GetUIController<VoiceChatGuideUIController>().Open(true);
    }
}
