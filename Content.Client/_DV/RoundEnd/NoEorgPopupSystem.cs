using Content.Shared.GameTicking;
using Content.Shared._DV.CCVars;
using Robust.Shared.Configuration;

namespace Content.Client._DV.RoundEnd;

public sealed class NoEorgPopupSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private NoEorgPopup? _window;
    private bool _skipPopup;
    private bool _eorgPopup;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, DCCVars.RoundEndNoEorgPopup, val => _eorgPopup = val, true);
        Subs.CVar(_cfg, DCCVars.SkipRoundEndNoEorgPopup, val => _skipPopup = val, true);

        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (_skipPopup || !_eorgPopup)
            return;

        OpenNoEorgPopup();
    }

    private void OpenNoEorgPopup()
    {
        if (_window != null)
            return;

        _window = new NoEorgPopup();
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
    }
}
