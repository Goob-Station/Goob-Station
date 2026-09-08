using Robust.Shared.Audio.Components;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Handles audio ducking, AKA, lowering the volume of any sounds that are not coming from the cinematic.
/// </summary>
public sealed partial class SharedCinematicSystem
{
    private void UpdateAudio(Entity<CinematicComponent> ent, CinematicPrototype timeline, EntityUid? viewer)
    {
        var playing = GetStrength(ent.Comp, timeline) > 0f;
        if (!playing)
            StopSegmentSounds(ent);

        if (playing && ent.Owner == viewer && timeline.DuckAudio)
            DuckOthers(ent, timeline.DuckDecibels);
        else
            RestoreDucked(ent);
    }

    private void DuckOthers(Entity<CinematicComponent> ent, float decibels)
    {
        var ducked = ent.Comp.Ducked;

        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out var uid, out var audio))
        {
            if (HasComp<CinematicSceneSoundComponent>(uid))
                continue;

            var volume = audio.Params.Volume;
            if (ducked.TryGetValue(uid, out var entry) && entry.Ducked.Equals(volume))
                continue;

            entry = new DuckedVolume(volume, volume - decibels);
            ducked[uid] = entry;
            _audio.SetVolume(uid, entry.Ducked, audio);
        }
    }

    private void RestoreDucked(Entity<CinematicComponent> ent)
    {
        foreach (var (uid, entry) in ent.Comp.Ducked)
        {
            if (!TryComp<AudioComponent>(uid, out var audio))
                continue;

            var volume = audio.Params.Volume;
            if (volume.Equals(entry.Ducked))
                _audio.SetVolume(uid, entry.Original, audio);
        }

        ent.Comp.Ducked.Clear();
    }

    private void StopSegmentSounds(Entity<CinematicComponent> ent)
    {
        foreach (var stream in ent.Comp.SegmentSounds)
            _audio.Stop(stream);

        ent.Comp.SegmentSounds.Clear();
    }
}
