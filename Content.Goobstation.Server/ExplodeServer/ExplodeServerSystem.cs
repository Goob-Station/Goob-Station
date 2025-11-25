using Content.Goobstation.Common.ExplodeServer;
using Content.Server.GameTicking;
using Content.Shared.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    private TimeSpan _roundEndTimer; // to restart the server

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndTimer < _gameTiming.CurTime && _roundEndTimer != TimeSpan.Zero)
        {
            _roundEndTimer = TimeSpan.Zero;
            _gameTicker.RestartRound(); // restart round now
        }
    }
    
    public void TriggerOverlay()
    {
        _gameTicker.EndRound();
        var audio = AudioParams.Default.WithVolume(1f);
        var specifier = _audio.ResolveSound(new SoundPathSpecifier("/Audio/_Goobstation/Announcements/explode_server_alert.ogg"));
        var ev = new GameGlobalSoundEvent(specifier, audio);
        RaiseNetworkEvent(ev);
        _roundEndTimer = _gameTiming.CurTime + TimeSpan.FromMilliseconds(5105);
        RaiseNetworkEvent(new ExplodeServerEvent());
    }
}
