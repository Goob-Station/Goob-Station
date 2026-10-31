using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.MaintsSounds;
public sealed class MaintsSoundsSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly SoundSpecifier Sounds = new SoundCollectionSpecifier("MaintsAmbience");

    private static readonly AudioParams SoundParams = AudioParams.Default
        .WithVolume(-2f)
        .WithVariation(0.3f);

    /// <summary>Range in tiles around the player to look for cat walks.</summary>
    private const float AreaRange = 8f;

    /// <summary>Min/max distance in tiles from the player at which the sound is heard.</summary>
    private const float MinSoundDistance = 3f;
    private const float MaxSoundDistance = 8f;

    /// <summary>Min/max delay in seconds between sounds for one player.</summary>
    private const float MinDelay = 360f;
    private const float MaxDelay = 490f;

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ActorComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out _, out var xform))
        {
            var listener = EnsureComp<MaintsSoundsListenerComponent>(uid);

            if (curTime < listener.NextSound)
                continue;

            ScheduleNext(listener, curTime);

            if (!_mobState.IsAlive(uid) || !IsInMaints(xform))
                continue;

            PlaySoundNear(uid, xform);
        }
    }

    private bool IsInMaints(TransformComponent xform)
    {
        return _lookup.GetEntitiesInRange<MaintsAreaMarkerComponent>(xform.Coordinates, AreaRange).Count > 0;
    }

    private void PlaySoundNear(EntityUid player, TransformComponent xform)
    {
        var offset = _random.NextAngle().ToVec() * _random.NextFloat(MinSoundDistance, MaxSoundDistance);
        var coords = xform.Coordinates.Offset(offset);

        _audio.PlayStatic(Sounds, Filter.Entities(player), coords, true, SoundParams);
    }

    private void ScheduleNext(MaintsSoundsListenerComponent listener, TimeSpan curTime)
    {
        listener.NextSound = curTime + TimeSpan.FromSeconds(_random.NextFloat(MinDelay, MaxDelay));
    }
}
