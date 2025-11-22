using Content.Goobstation.Common.StationReport;
using Content.Goobstation.Shared.ExplodeServer;
using Content.Server.Audio;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
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
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private TimeSpan _roundEndTimer; // to restart the server
    private bool _triggered;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndTimer < _gameTiming.CurTime && _triggered)
        {
            _triggered = false; // it kept restarting in a loop otherwise
            _entManager.System<GameTicker>().RestartRound(); // restart round now
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        _triggered = false;
        SubscribeLocalEvent<StationReportEvent>(OnRoundEnd);
    }

    public void TriggerOverlay()
    {
        _entManager.System<RoundEndSystem>().EndRound();
        _triggered = true;
        Filter filter;
        var audio = AudioParams.Default;
        audio.Volume = 1f;
        var replay = true;
        var path = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/ExplodeServerAlert.ogg");
        var soundEffect = _audio.ResolveSound(path);
        filter = Filter.Empty().AddAllPlayers(_playerManager);
        _entManager.System<ServerGlobalSoundSystem>().PlayAdminGlobal(filter, soundEffect, audio, replay);
        _roundEndTimer = _gameTiming.CurTime + TimeSpan.FromMilliseconds(5105);
        RaiseNetworkEvent(new ExplodeServerEvent());
    }

    private void OnRoundEnd(StationReportEvent ev)
    {
        _triggered = true;// to prevent multiple triggers
        if (_random.Prob(0.01f) && !_triggered) // 1% chance to trigger explode server
        {
            TriggerOverlay();
        }
    }
}