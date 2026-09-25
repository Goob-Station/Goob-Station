using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Ghost;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// The audio side of the fear system.
/// </summary>
public sealed partial class SlasherFearSystem
{
    private readonly Dictionary<EntityUid, (float DropPerSecond, float NormalVolume, float SilentVolume, EntityUid? Victim)> _fadingMusic = new();

    private readonly List<EntityUid> _fadedOut = new();

    private EntityUid? _observerStream;

    /// <summary>
    /// Plays the jumpscare stinger for the Slasher and the victim.
    /// </summary>
    private void Jumpscare(EntityUid slasher, EntityUid victim, SlasherFearComponent comp)
    {
        if (!_net.IsServer)
            return;

        var filter = Filter.Empty();
        if (_player.TryGetSessionByEntity(slasher, out var slasherSession))
            filter.AddPlayer(slasherSession);
        if (_player.TryGetSessionByEntity(victim, out var victimSession))
            filter.AddPlayer(victimSession);

        var sound = comp.JumpscareSound;
        if (comp.JumpscareSounds.Count > 0)
            sound = _random.Pick(comp.JumpscareSounds);

        _audio.PlayGlobal(sound, filter, false);
    }

    private void OnToggleMusic(Entity<SlasherFearComponent> ent, ref SlasherToggleFearMusicAlertEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.MusicMuted = !ent.Comp.MusicMuted;
        Dirty(ent);

        var message = ent.Comp.MusicMuted ? "slasher-fear-music-muted" : "slasher-fear-music-unmuted";
        _popup.PopupClient(Loc.GetString(message), ent, ent);
        args.Handled = true;
    }

    private void UpdateMusic(Entity<SlasherFearComponent> ent)
    {
        var (uid, comp) = ent;

        var active = comp.Meter > 0f && CanHunt(uid);
        if (comp.MusicActive != active)
        {
            comp.MusicActive = active;
            Dirty(uid, comp);
        }

        if (!_timing.IsFirstTimePredicted || _player.LocalEntity != uid)
            return;

        if (comp.MusicActive && !comp.MusicMuted)
            comp.MusicStream = StartOrResumeMusic(comp.MusicStream, comp.BloodTrailMusic);
        else if (comp.MusicStream is { } stream)
        {
            if (Exists(stream))
                FadeOutMusic(stream, comp);
            else
                comp.MusicStream = null;
        }
    }

    /// <summary>
    /// Fades out the slasher's own copy of the theme, e.g. when they lose the component or leave their body.
    /// </summary>
    private void StopSlasherMusic(Entity<SlasherFearComponent> ent)
    {
        if (ent.Comp.MusicStream is { } stream)
            FadeOutMusic(stream, ent.Comp);
        ent.Comp.MusicStream = null;
    }

    private void UpdateVictimMusic(Entity<FearedComponent> ent)
    {
        var (uid, comp) = ent;

        if (!_timing.IsFirstTimePredicted || _player.LocalEntity != uid)
            return;

        var handoff = comp.MusicScarer != comp.Scarer;
        if (handoff)
        {
            if (comp.MusicStream is { } previous && Exists(previous))
            {
                TryComp<SlasherFearComponent>(comp.MusicScarer, out var previousTrail);
                FadeOutMusic(previous, previousTrail);
            }

            comp.MusicStream = null;
            comp.MusicScarer = comp.Scarer;
        }

        if (!(comp.MusicStream is { } current && Exists(current))
            && TryComp<SlasherFearComponent>(comp.Scarer, out var trail))
            comp.MusicStream = StartOrResumeMusic(handoff ? null : FindVictimFade(uid), trail.BloodTrailMusic);
    }

    /// <summary>
    /// Fades out the victim's copy of the hunt theme when they stop being feared or leave their body.
    /// </summary>
    private void FadeVictimMusic(Entity<FearedComponent> ent)
    {
        if (ent.Comp.MusicStream is { } stream)
        {
            TryComp<SlasherFearComponent>(ent.Comp.MusicScarer, out var trail);
            FadeOutMusic(stream, trail, ent.Owner);
        }
        ent.Comp.MusicStream = null;
    }

    /// <summary>
    /// Finds the stream currently fading out, if any.
    /// </summary>
    private EntityUid? FindVictimFade(EntityUid victim)
    {
        foreach (var (stream, fade) in _fadingMusic)
            if (fade.Victim == victim)
                return stream;

        return null;
    }

    /// <summary>
    /// Resumes the music if it was active and fading (raises the volume back up) or starts it if there isn't one.
    /// </summary>
    private EntityUid? StartOrResumeMusic(EntityUid? existing, SoundSpecifier music)
    {
        if (existing is { } stream && Exists(stream))
        {
            ResumeMusic(stream);
            return stream;
        }

        return _audio.PlayGlobal(music, Filter.Local(), false)?.Entity;
    }

    /// <summary>
    /// Lets an observing ghost hear the music.
    /// </summary>
    private void UpdateObserverMusic()
    {
        var slasher = _player.LocalEntity is { } local && HasComp<GhostComponent>(local)
            ? FindHuntingSlasher()
            : null;

        if (slasher is { } target && TryComp<SlasherFearComponent>(target, out var comp))
        {
            if (_observerStream is not { } playing || !Exists(playing))
                _observerStream = _audio.PlayEntity(comp.BloodTrailMusic, Filter.Local(), target, false)?.Entity;

            return;
        }

        if (_observerStream is { } stream)
        {
            if (Exists(stream) && TryComp<SlasherFearComponent>(Transform(stream).ParentUid, out var oldComp))
                FadeOutMusic(stream, oldComp);
            else if (Exists(stream))
                QueueDel(stream);

            _observerStream = null;
        }
    }

    /// <summary>
    /// Returns any slasher whose hunt theme is currently active.
    /// </summary>
    private EntityUid? FindHuntingSlasher()
    {
        var query = EntityQueryEnumerator<SlasherFearComponent>();
        while (query.MoveNext(out var uid, out var comp))
            if (comp.MusicActive)
                return uid;

        return null;
    }

    /// <summary>
    /// Slowly fades out the music.
    /// </summary>
    private void FadeOutMusic(EntityUid stream, SlasherFearComponent? trail, EntityUid? victim = null)
    {
        if (_fadingMusic.ContainsKey(stream)
            || !TryComp<AudioComponent>(stream, out var audio))
            return;

        var drop = trail == null ? 0f : audio.Volume - trail.MusicSilentVolume;
        if (trail == null || drop <= 0f)
        {
            QueueDel(stream);
            return;
        }

        _fadingMusic[stream] = (drop / trail.MusicFadeDuration, audio.Volume, trail.MusicSilentVolume, victim);
    }

    /// <summary>
    /// Cancels an in-progress fade and snaps the stream back to its full volume.
    /// </summary>
    private void ResumeMusic(EntityUid stream)
    {
        if (!_fadingMusic.Remove(stream, out var fade))
            return;

        if (TryComp<AudioComponent>(stream, out var audio))
            _audio.SetVolume(stream, fade.NormalVolume, audio);
    }

    private void UpdateMusicFades(float frameTime)
    {
        if (_fadingMusic.Count == 0)
            return;

        foreach (var (stream, fade) in _fadingMusic)
        {
            if (!TryComp<AudioComponent>(stream, out var audio))
            {
                _fadedOut.Add(stream);
                continue;
            }

            var volume = MathF.Max(fade.SilentVolume, audio.Volume - fade.DropPerSecond * frameTime);
            _audio.SetVolume(stream, volume, audio);

            if (volume <= fade.SilentVolume)
            {
                QueueDel(stream);
                _fadedOut.Add(stream);
            }
        }

        foreach (var stream in _fadedOut)
            _fadingMusic.Remove(stream);
        _fadedOut.Clear();
    }
}
