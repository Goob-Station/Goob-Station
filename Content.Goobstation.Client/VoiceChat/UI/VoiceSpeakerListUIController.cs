using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Common.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed class VoiceSpeakerListUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [UISystemDependency] private readonly VoiceChatSystem? _voice = default;

    private VoiceSpeakerList? _list;
    private UIScreen? _attachedScreen;
    private bool _inGameplay;
    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(GoobCVars.VoiceChatSpeakerList, OnEnabledChanged, true);
    }

    public void OnStateEntered(GameplayState state)
    {
        _inGameplay = true;
    }

    public void OnStateExited(GameplayState state)
    {
        _inGameplay = false;
        Detach();
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_inGameplay || !_enabled || _voice == null)
            return;

        if (UIManager.ActiveScreen != _attachedScreen || _list?.Parent == null)
            Attach(UIManager.ActiveScreen);

        _list?.Update(args.DeltaSeconds, _voice);
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
