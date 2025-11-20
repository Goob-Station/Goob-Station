using Content.Goobstation.Shared.ExplodeServer;
using Content.Server.Audio;
using Content.Server.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    

    public void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ExplodeServerEvent>(OnCountdownEnd);
    }

    public void TriggerOverlay()
    {
        Filter filter;
        var audio = AudioParams.Default;
        bool replay = true;
        var path = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/ExplodeServerAlert.ogg");
        var soundEffect = _audio.ResolveSound(path);
        filter = Filter.Empty().AddAllPlayers(_playerManager);
        _entManager.System<ServerGlobalSoundSystem>().PlayAdminGlobal(filter, soundEffect , audio, replay);
        RaiseNetworkEvent(new ExplodeServerEvent());
    }

    private void OnCountdownEnd(ExplodeServerEvent e)
    {
        _entManager.System<GameTicker>().RestartRound();
    }
}
