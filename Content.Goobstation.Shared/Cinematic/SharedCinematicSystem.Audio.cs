using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Cinematic;

public sealed partial class SharedCinematicSystem
{
    private readonly List<SoundFade> _fades = new();

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

    public void FadeOut(EntityUid stream, float seconds)
    {
        if (seconds <= 0f || !TryComp<AudioComponent>(stream, out var audio))
        {
            _audio.Stop(stream);
            return;
        }

        _fades.Add(new SoundFade(stream, seconds, audio.Params.Volume));
    }

    private void UpdateFades(float frameTime)
    {
        for (var i = _fades.Count - 1; i >= 0; i--)
        {
            var fade = _fades[i];
            fade.Elapsed += frameTime;

            var progress = fade.Elapsed / fade.Duration;
            if (progress >= 1f || !TryComp<AudioComponent>(fade.Stream, out var audio))
            {
                _audio.Stop(fade.Stream);
                _fades.RemoveAt(i);
                continue;
            }

            _audio.SetVolume(fade.Stream, fade.StartVolume + SharedAudioSystem.GainToVolume(1f - progress), audio);
            _fades[i] = fade;
        }
    }

    private record struct SoundFade(EntityUid Stream, float Duration, float StartVolume)
    {
        public float Elapsed;
    }

}
