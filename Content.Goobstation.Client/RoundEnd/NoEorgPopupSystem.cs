using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Content.Goobstation.Common.CCVar;

namespace Content.Goobstation.Client.RoundEnd;

public sealed class NoEorgPopupSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private NoEorgPopup? _window;
    private bool _eorgPopup;
    private int _neededTime;
    private string _lastShown = "";

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, GoobCVars.RoundEndNoEorgPopup, val => _eorgPopup = val, true);
        Subs.CVar(_cfg, GoobCVars.AskRoundEndNoEorgPopup, val => _neededTime = val, true);
        Subs.CVar(_cfg, GoobCVars.LastReadRoundEndNoEorgPopup, val => _lastShown = val, true);

        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (!_eorgPopup)
            return;

        var now = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(_lastShown))
        {
            if (DateTime.TryParse(_lastShown, out var lastShownDate))
            {
                var timeSinceLastShown = (int) (now - lastShownDate).TotalDays;

                if (timeSinceLastShown < _neededTime)
                    return;
            }
        }

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
