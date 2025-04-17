using Content.Client.Audio;
using Content.Shared._Corvax.Speech.Synthesis;
using Content.Shared.Chat;
using Content.Shared.Corvax.CorvaxVars;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Corvax.Speech.Synthesis.System;

/// <summary>
/// The system responsible for the sound transmission for each subscriber
/// </summary>
public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private const float MinimalVolume = -10f;
    private const float WhisperFade = 4f;

    public override void Initialize()
    {
        SubscribeNetworkEvent<PlayBarkEvent>(OnPlayBark);
    }

    public void RequestPreviewBark(string barkVoiceId)
    {
        RaiseNetworkEvent(new RequestPreviewBarkEvent(barkVoiceId));
    }

    private void OnPlayBark(PlayBarkEvent ev)
    {
        var sourceEntity = _entityManager.GetEntity(ev.SourceUid);
        if (!_entityManager.EntityExists(sourceEntity) || _entityManager.Deleted(sourceEntity) || !HasComp<TransformComponent>(sourceEntity))
            return;

        if (_player.LocalEntity != null && HasComp<TransformComponent>(_player.LocalEntity.Value))
        {
            var sourceTransform = Transform(sourceEntity);
            var playerTransform = Transform(_player.LocalEntity.Value);

            if (sourceTransform.Coordinates.TryDistance(EntityManager, playerTransform.Coordinates, out var distance) &&
                distance > SharedChatSystem.VoiceRange)
                return;
        }

        var userVolume = _cfg.GetCVar(CorvaxVars.BarksVolume);
        var baseVolume = SharedAudioSystem.GainToVolume(userVolume * ContentAudioSystem.BarksMultiplier);

        float volume = MinimalVolume + baseVolume;
        if (ev.Obfuscated) volume -= WhisperFade;

        var audioParams = new AudioParams
        {
            Volume = volume,
            Variation = 0.125f
        };

        int messageLength = ev.Message.Length;
        float totalDuration = messageLength * 0.05f;
        float soundInterval = 0.15f / ev.PlaybackSpeed;

        int soundCount = (int)(totalDuration / soundInterval);
        soundCount = Math.Max(soundCount, 1);

        var audioResource = new AudioResource();
        audioResource.Load(IoCManager.Instance!, new ResPath(ev.SoundPath));

        var soundSpecifier = new ResolvedPathSpecifier(ev.SoundPath);

        for (int i = 0; i < soundCount; i++)
        {
            Timer.Spawn(TimeSpan.FromSeconds(i * soundInterval), () =>
            {
                if (!_entityManager.EntityExists(sourceEntity) || _entityManager.Deleted(sourceEntity))
                    return;

                _audio.PlayEntity(audioResource.AudioStream, sourceEntity, soundSpecifier, audioParams);
            });
        }
    }
}
