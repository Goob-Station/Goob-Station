using Content.Goobstation.Common.ExplodeServer;
using Content.Server.Audio;
using Content.Server.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _globalSound = default!;
    private TimeSpan? _roundEndTimer; // to restart the server

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndTimer < _gameTiming.CurTime && _roundEndTimer != null)
        {
            _roundEndTimer = null;
            _gameTicker.RestartRound(); // restart round now
        }
    }
    
    public void TriggerOverlay()
    {
        _gameTicker.EndRound();
        var audio = AudioParams.Default;
        audio.Volume = 1f;
        _globalSound.PlayAdminGlobal(Filter.Empty().AddAllPlayers(_playerManager), _audio.ResolveSound(new SoundPathSpecifier("/Audio/_Goobstation/Announcements/ExplodeServerAlert.ogg")), AudioParams.Default.WithVolume(1f)); 
        _roundEndTimer = _gameTiming.CurTime + TimeSpan.FromMilliseconds(5105);
        RaiseNetworkEvent(new ExplodeServerEvent());
    }
}

