using Content.Client.Audio;
using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Audio;
using Content.Shared.CCVar;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Lavaland.Audio;

public sealed class BossMusicSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly ContentAudioSystem _audioContent = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private static float _volumeSlider;
    private EntityUid? _bossMusicStream;
    private Entity<BossMusicComponent>? _bossMusicOrigin;
    private BossMusicPrototype? _musicProto;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configManager, CCVars.AmbientMusicVolume, AmbienceCVarChanged, true);
        SubscribeLocalEvent<BossMusicComponent, AggressorAddedEvent>(StartBossMusic);
        SubscribeLocalEvent<BossMusicComponent, AggressorRemovedEvent>(EndBossMusic);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _bossMusicStream = _audio.Stop(_bossMusicStream);
    }

    private void AmbienceCVarChanged(float obj)
    {
        _volumeSlider = SharedAudioSystem.GainToVolume(obj);

        if (_bossMusicStream != null && _musicProto != null)
        {
            _audio.SetVolume(_bossMusicStream, _musicProto.Sound.Params.Volume + _volumeSlider);
        }
    }

    private void StartBossMusic(Entity<BossMusicComponent> ent, ref AggressorAddedEvent args)
    {
        var player = _player.LocalSession?.AttachedEntity;
        var agressor = GetEntity(args.Aggressor);

        if (agressor != player || _musicProto != null || _bossMusicStream != null)
            return;

        _audioContent.DisableAmbientMusic();

        var sound = _proto.Index(ent.Comp.SoundId);
        _musicProto = sound;

        var strim = _audio.PlayGlobal(
            sound.Sound,
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(sound.Sound.Params.Volume + _volumeSlider));

        _bossMusicStream = strim?.Entity;

        if (_musicProto.FadeIn && strim != null)
        {
            _audioContent.FadeIn(_bossMusicStream, strim.Value.Component, sound.FadeInTime);
        }

        _bossMusicOrigin = ent;
    }

    private void EndBossMusic(Entity<BossMusicComponent> ent, ref AggressorRemovedEvent args)
    {
        var player = _player.LocalSession?.AttachedEntity;
        var agressor = GetEntity(args.Aggressor);

        if (agressor != player || _bossMusicOrigin != ent || _musicProto == null || _bossMusicStream == null)
            return;

        if (_musicProto.PositionOnEnd != null)
            _audio.SetPlaybackPosition(_bossMusicStream, _musicProto.PositionOnEnd.Value);

        if (_musicProto.FadeIn)
        {

            _audioContent.FadeOut(_bossMusicStream, duration: _musicProto.FadeOutTime);
        }
        else
        {
            _audio.Stop(_bossMusicStream);
        }

        _musicProto = null;
        _bossMusicStream = null;
    }
}
