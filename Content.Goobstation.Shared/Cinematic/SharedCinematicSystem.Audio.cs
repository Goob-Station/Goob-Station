using Robust.Shared.Audio.Components;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Handles audio ducking, AKA, lowering the volume of any sounds that aren't coming from the cinematic.
/// </summary>
public sealed partial class SharedCinematicSystem
{
    private void UpdateAudio(Entity<CinematicComponent> ent, EntityUid? viewer)
    {
        if (!IsPlayingFor(ent, viewer) || !_proto.TryIndex(ent.Comp.Timeline, out var timeline))
        {
            StopSegmentSounds(ent);
            RestoreDucked(ent);
            return;
        }

        if (timeline.DuckAudio)
            DuckOthers(ent, timeline.DuckDecibels);
    }

    private bool IsPlayingFor(Entity<CinematicComponent> ent, EntityUid? viewer) =>
        ent.Owner == viewer && GetStrength(ent.Comp) > 0f;

    private void DuckOthers(Entity<CinematicComponent> ent, float decibels)
    {
        var ducked = ent.Comp.Ducked;

        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out var uid, out var audio))
        {
            if (HasComp<CinematicSceneSoundComponent>(uid) || ducked.ContainsKey(uid))
                continue;

            ducked[uid] = audio.Params.Volume;
            _audio.SetVolume(uid, audio.Params.Volume - decibels);
        }
    }

    private void RestoreDucked(Entity<CinematicComponent> ent)
    {
        foreach (var (uid, volume) in ent.Comp.Ducked)
            if (HasComp<AudioComponent>(uid))
                _audio.SetVolume(uid, volume);

        ent.Comp.Ducked.Clear();
    }

    private void StopSegmentSounds(Entity<CinematicComponent> ent)
    {
        foreach (var stream in ent.Comp.SegmentSounds)
            _audio.Stop(stream);

        ent.Comp.SegmentSounds.Clear();
    }
}
