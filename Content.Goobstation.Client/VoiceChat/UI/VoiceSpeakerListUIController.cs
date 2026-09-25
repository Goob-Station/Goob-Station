using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Common.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed class VoiceSpeakerListUIController : UIController,
    IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    private const float Margin = 10f;
    private const float GameplayBottom = 110f;

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [UISystemDependency] private readonly VoiceChatSystem? _voice = default;

    private VoiceSpeakerList? _list;
    private UIScreen? _attachedScreen;
    private bool _active;
    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(GoobCVars.VoiceChatSpeakerList, OnEnabledChanged, true);
    }

    public void OnStateEntered(GameplayState state)
    {
        _active = true;
    }

    public void OnStateExited(GameplayState state)
    {
        _active = false;
        Detach();
    }

    public void OnStateEntered(LobbyState state)
    {
        _active = true;
    }

    public void OnStateExited(LobbyState state)
    {
        _active = false;
        Detach();
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_active || !_enabled || _voice == null)
            return;

        if (UIManager.ActiveScreen != _attachedScreen || _list?.Parent == null)
            Attach(UIManager.ActiveScreen);

        if (_list == null)
            return;

        if (_attachedScreen is LobbyGui lobby)
            _list.SetInset(lobby.RightSide.Width + Margin, Margin);
        else
            _list.SetInset(Margin, GameplayBottom);

        _list.Update(args.DeltaSeconds, _voice);
    }

    private void OnEnabledChanged(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
            Detach();
    }

    private void Attach(UIScreen? screen)
    {
        Detach();
        if (screen == null)
            return;

        LayoutContainer container = screen;
        if (screen.GetWidget<MainViewport>() is { Parent: LayoutContainer viewportContainer })
            container = viewportContainer;

        _list ??= new VoiceSpeakerList();
        container.AddChild(_list);
        LayoutContainer.SetAnchorPreset(_list, LayoutContainer.LayoutPreset.Wide);
        _attachedScreen = screen;
    }

    private void Detach()
    {
        _list?.Clear();
        _list?.Orphan();
        _attachedScreen = null;
    }
}
