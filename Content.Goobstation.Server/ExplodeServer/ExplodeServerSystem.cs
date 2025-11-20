using Content.Goobstation.Shared.ExplodeServer;
using Content.Server.Audio;
using Content.Server.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    private TimeSpan _roundEndTimer; // to restart the server

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndTimer < _gameTiming.CurTime)
            _entManager.System<GameTicker>().RestartRound();
    }
    public void TriggerOverlay()
    {
        Filter filter;
        var audio = AudioParams.Default;
        audio.Volume = 1f;
        bool replay = true;
        var path = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/ExplodeServerAlert.ogg");
        var soundEffect = _audio.ResolveSound(path);
        filter = Filter.Empty().AddAllPlayers(_playerManager);
        _entManager.System<ServerGlobalSoundSystem>().PlayAdminGlobal(filter, soundEffect, audio, replay);
        _roundEndTimer = _gameTiming.CurTime + TimeSpan.FromMilliseconds(5105)
        RaiseNetworkEvent(new ExplodeServerEvent());
    }
}
