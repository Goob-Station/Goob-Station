using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Alert;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Fluids;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the slashers jumpscares / music / fear meter / fear overlay / blood trail / etc.
/// </summary>
public sealed class SlasherFearSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPuddleSystem _puddles = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementModStatusSystem _movemod = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _moveSpeed = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private readonly Dictionary<EntityUid, (float DropPerSecond, float NormalVolume, float SilentVolume, EntityUid? Victim)> _fadingMusic = new();

    private readonly List<EntityUid> _fadedOut = new();

    private readonly Dictionary<EntityUid, TimeSpan> _nextDropAt = new();

    private EntityUid? _observerStream;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherFearComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SlasherFearComponent, ComponentStartup>(OnFearStartup);
        SubscribeLocalEvent<SlasherFearComponent, ComponentShutdown>(OnFearShutdown);
        SubscribeLocalEvent<SlasherFearComponent, LocalPlayerDetachedEvent>(OnFearDetached);
        SubscribeLocalEvent<FearedComponent, ComponentShutdown>(OnFearedShutdown);
        SubscribeLocalEvent<FearedComponent, LocalPlayerDetachedEvent>(OnFearedDetached);
    }

    private void OnRefreshSpeed(Entity<SlasherFearComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.SpeedBoostActive)
            return;

        var mod = 1f + ent.Comp.MaxSpeedBonus;
        args.ModifySpeed(mod, mod);
    }

    private void OnFearStartup(Entity<SlasherFearComponent> ent, ref ComponentStartup args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.SeenAlert, 0);
    }

    private void OnFearShutdown(Entity<SlasherFearComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.Alert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.SeenAlert);
        _nextDropAt.Remove(ent.Owner);

        if (ent.Comp.MusicStream is { } stream)
            FadeOutMusic(stream, ent.Comp);
        ent.Comp.MusicStream = null;

        var victims = EntityQueryEnumerator<FearedComponent>();
        while (victims.MoveNext(out var victimUid, out var victim))
        {
            if (victim.Scarer != ent.Owner)
                continue;

            victim.Scarer = null;
            Dirty(victimUid, victim);
        }
    }

    private void OnFearDetached(Entity<SlasherFearComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        if (ent.Comp.MusicStream is { } stream)
            FadeOutMusic(stream, ent.Comp);
        ent.Comp.MusicStream = null;
    }

    private void OnFearedDetached(Entity<FearedComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        FadeVictimMusic(ent);
    }

    private void OnFearedShutdown(Entity<FearedComponent> ent, ref ComponentShutdown args)
    {
        FadeVictimMusic(ent);

        if (_net.IsServer)
            ClearFearStyle(ent);
    }

    /// <summary>
    /// Ensures the scarer's fear-style components on a victim.
    /// </summary>
    private void ApplyFearStyle(Entity<FearedComponent> victim, ComponentRegistry style)
    {
        ClearFearStyle(victim);

        if (style.Count == 0)
            return;

        EntityManager.AddComponents(victim.Owner, style);
        victim.Comp.AppliedStyle = style;
    }

    /// <summary>
    /// Removes the style components previously ensured on a victim, if any.
    /// </summary>
    private void ClearFearStyle(Entity<FearedComponent> victim)
    {
        if (victim.Comp.AppliedStyle is not { } style)
            return;

        EntityManager.RemoveComponents(victim.Owner, style);
        victim.Comp.AppliedStyle = null;
    }

    /// <summary>
    /// Fades out the victim's copy of the hunt theme when they stop being feared or leave their body.
    /// </summary>
    private void FadeVictimMusic(Entity<FearedComponent> ent)
    {
        var trail = ent.Comp.MusicScarer is { } scarer ? CompOrNull<SlasherFearComponent>(scarer) : null;
        if (ent.Comp.MusicStream is { } stream)
            FadeOutMusic(stream, trail, ent.Owner);
        ent.Comp.MusicStream = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.IsFirstTimePredicted)
        {
            UpdateMusicFades(frameTime);
            UpdateObserverMusic();
        }

        var now = _timing.CurTime;

        var slashers = EntityQueryEnumerator<SlasherFearComponent>();
        while (slashers.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextCheck)
                continue;

            comp.NextCheck = now + comp.CheckInterval;
            Scan((uid, comp), now);
        }

        var feared = EntityQueryEnumerator<FearedComponent>();
        while (feared.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            comp.NextUpdate = now + comp.UpdateInterval;
            UpdateVictim((uid, comp), now);
        }

        if (_net.IsServer)
            UpdateBloodTrails(now);
    }

    /// <summary>
    /// Drips blood puddles at the feet of every hunting slasher whose meter has crossed the blood threshold.
    /// </summary>
    private void UpdateBloodTrails(TimeSpan now)
    {
        var enumerator = EntityQueryEnumerator<SlasherFearComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsActive)
            {
                _nextDropAt.Remove(uid);
                continue;
            }

            if (!_nextDropAt.TryGetValue(uid, out var next))
            {
                _nextDropAt[uid] = now;
                continue;
            }

            if (now < next)
                continue;

            _nextDropAt[uid] = now + comp.DropInterval;

            var solution = new Solution();
            var amount = FixedPoint2.Max(FixedPoint2.Zero, comp.VolumePerDrop);
            solution.AddReagent(comp.BloodTrailReagent, amount);

            _puddles.TrySpillAt(uid, solution, out _, sound: false);
        }
    }

    private void Scan(Entity<SlasherFearComponent> ent, TimeSpan now)
    {
        var (uid, comp) = ent;
        var canHunt = CanHunt(uid);
        var seen = new HashSet<EntityUid>();

        foreach (var other in _lookup.GetEntitiesInRange(uid, comp.Range))
        {
            if (!IsValidVictim(uid, other)
                || !_interaction.InRangeUnobstructed(uid, other, comp.Range, CollisionGroup.Opaque))
                continue;

            seen.Add(other);

            if (!canHunt)
                continue;

            if (_net.IsServer && !_status.HasStatusEffect(other, comp.FearedEffect))
                _status.TryAddStatusEffect(other, comp.FearedEffect, out _, duration: null);

            if (!TryComp<FearedComponent>(other, out var victim))
                continue;

            victim.SourceEffect = comp.FearedEffect;

            // This stops evil things from happening if there's more than 1 slasher.
            if (victim.Scarer is { } owner
                && owner != uid
                && !TerminatingOrDeleted(owner)
                && HasComp<SlasherFearComponent>(owner)
                && now - victim.LastObserved < victim.OwnershipTimeout)
                continue;

            if (victim.Scarer != uid)
            {
                victim.Scarer = uid;
                if (_net.IsServer)
                    ApplyFearStyle((other, victim), comp.FearStyle);
            }

            victim.LastObserved = now;

            var isNewPerson = !comp.Observing.Contains(other);
            if (isNewPerson && now >= comp.NextJumpscare)
            {
                comp.NextJumpscare = now + comp.JumpscareCooldown;
                comp.Meter = MathF.Min(comp.MaxMeter, comp.Meter + comp.MeterPerJumpscare);
                victim.Fear = MathF.Min(1f, victim.Fear + comp.JumpscareFear);

                if (_net.IsServer)
                    Jumpscare(uid, other, comp);
            }

            Dirty(other, victim);
        }
        var anySeen = seen.Count > 0;
        var dt = (float) comp.CheckInterval.TotalSeconds;

        SetObserved(ent, anySeen);

        if (!canHunt)
        {
            comp.Observing.Clear();
            comp.Meter = MathF.Max(0f, comp.Meter - comp.MeterDecayPerSecond * dt);
            Dirty(uid, comp);
            UpdateHuntEffects(ent);
            return;
        }

        if (anySeen)
        {
            comp.LastSeenVictim = now;
            comp.Meter = MathF.Min(comp.MaxMeter, comp.Meter + comp.MeterPassivePerSecond * dt);
        }
        else if (now - comp.LastSeenVictim >= comp.MeterGracePeriod)
            comp.Meter = MathF.Max(0f, comp.Meter - comp.MeterDecayPerSecond * dt);

        comp.Observing = seen;
        Dirty(uid, comp);
        UpdateHuntEffects(ent);
    }

    private void UpdateVictim(Entity<FearedComponent> ent, TimeSpan now)
    {
        var (uid, comp) = ent;

        var sinceObserved = now - comp.LastObserved;
        var observed = sinceObserved < comp.ObserveTimeout;

        if (observed)
            comp.Fear = MathF.Min(1f, comp.Fear + comp.GainPerSecond);
        else if (sinceObserved >= comp.FearGracePeriod)
            comp.Fear = MathF.Max(0f, comp.Fear - comp.DecayPerSecond);

        if (comp.Fear <= 0f && !observed)
        {
            if (_net.IsServer)
                _status.TryRemoveStatusEffect(uid, comp.SourceEffect);
            return;
        }

        if (comp.Fear >= comp.SlowThreshold)
            _movemod.TryUpdateMovementSpeedModDuration(
                uid, comp.SlowEffect, comp.SlowRefresh, comp.SlowMultiplier, comp.SlowMultiplier);

        if (comp.Fear >= comp.DamageThreshold && _net.IsServer)
            _damageable.TryChangeDamage(uid, comp.DamagePerSecond, true);

        Dirty(uid, comp);

        if (_timing.IsFirstTimePredicted && _player.LocalEntity == uid)
        {
            var handoff = comp.MusicScarer != comp.Scarer;
            if (handoff)
            {
                if (comp.MusicStream is { } previous && Exists(previous))
                {
                    var previousTrail = comp.MusicScarer is { } prevScarer
                        ? CompOrNull<SlasherFearComponent>(prevScarer)
                        : null;
                    FadeOutMusic(previous, previousTrail);
                }

                comp.MusicStream = null;
                comp.MusicScarer = comp.Scarer;
            }

            if (!(comp.MusicStream is { } current && Exists(current))
                && TryComp<SlasherFearComponent>(comp.Scarer, out var trail))
            {
                comp.MusicStream = StartOrResumeMusic(handoff ? null : FindVictimFade(uid), trail.BloodTrailMusic);
            }
        }
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
    /// Plays the jumpscare stinger for the Slasher and the victim.
    /// </summary>
    private void Jumpscare(EntityUid slasher, EntityUid victim, SlasherFearComponent comp)
    {
        var filter = Filter.Empty();
        if (_player.TryGetSessionByEntity(slasher, out var slasherSession))
            filter.AddPlayer(slasherSession);
        if (_player.TryGetSessionByEntity(victim, out var victimSession))
            filter.AddPlayer(victimSession);

        var sound = comp.JumpscareSound;
        if (comp.JumpscareSounds.Count > 0)
            sound = _random.Pick(comp.JumpscareSounds);

        _audio.PlayGlobal(sound, filter, true);
    }

    /// <summary>
    /// Pushes the current meter out to the alert, the Slasher's speed, the blood trail and the theme each scan.
    /// </summary>
    private void UpdateHuntEffects(Entity<SlasherFearComponent> ent)
    {
        var (uid, comp) = ent;

        var boosted = comp.Meter >= comp.MaxMeter;
        if (comp.SpeedBoostActive != boosted)
        {
            comp.SpeedBoostActive = boosted;
            Dirty(uid, comp);
            _moveSpeed.RefreshMovementSpeedModifiers(uid);
        }

        if (comp.Meter > 0f)
            _alerts.ShowAlert(uid, comp.Alert);
        else
            _alerts.ClearAlert(uid, comp.Alert);

        var bleeding = comp.Meter >= comp.BloodMeterThreshold;
        if (comp.IsActive != bleeding)
        {
            comp.IsActive = bleeding;
            Dirty(uid, comp);
        }

        UpdateMusic((uid, comp));
    }

    /// <summary>
    /// Drives the slashers looping theme.
    /// </summary>
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

        if (comp.MusicActive)
        {
            comp.MusicStream = StartOrResumeMusic(comp.MusicStream, comp.BloodTrailMusic);
        }
        else if (comp.MusicStream is { } stream)
        {
            if (Exists(stream))
                FadeOutMusic(stream, comp);
            else
                comp.MusicStream = null;
        }
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

    /// <summary>
    /// Ticks down the volume of every fading music stream, stopping it once it's silent.
    /// </summary>
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

    private void SetObserved(Entity<SlasherFearComponent> ent, bool observed)
    {
        if (ent.Comp.IsObserved != observed)
        {
            ent.Comp.IsObserved = observed;
            Dirty(ent);
        }

        _alerts.ShowAlert(ent.Owner, ent.Comp.SeenAlert, (short) (observed ? 1 : 0));
    }

    /// <summary>
    /// On-demand check whether any valid player currently has line of sight on the slasher.
    /// </summary>
    public bool IsObservedByPlayers(EntityUid uid, float range)
    {
        foreach (var other in _lookup.GetEntitiesInRange(uid, range))
        {
            if (other == uid
                || !HasComp<EyeComponent>(other)
                || HasComp<GhostComponent>(other)
                || !HasComp<HumanoidAppearanceComponent>(other)
                || _mobState.IsDead(other)
                || _mobState.IsCritical(other)
                || TryComp<BlindableComponent>(other, out var blind) && blind.IsBlind
                || TryComp<SlasherIncorporealComponent>(other, out var otherSlasher) && otherSlasher.IsIncorporeal)
                continue;

            if (_interaction.InRangeUnobstructed(other, uid, range, CollisionGroup.Opaque))
                return true;
        }

        return false;
    }

    private bool CanHunt(EntityUid uid)
    {
        if (_mobState.IsIncapacitated(uid)
            || TryComp<SlasherIncorporealComponent>(uid, out var inc) && inc.IsIncorporeal)
            return false;

        return true;
    }

    private static float Power(SlasherFearComponent comp)
    {
        return comp.MaxMeter <= 0f ? 0f : Math.Clamp(comp.Meter / comp.MaxMeter, 0f, 1f);
    }


    private bool IsValidVictim(EntityUid slasher, EntityUid other)
    {
        if (other == slasher
            || HasComp<SlasherComponent>(other)
            || !HasComp<EyeComponent>(other)
            || HasComp<GhostComponent>(other)
            || !HasComp<HumanoidAppearanceComponent>(other)
            || !_mind.TryGetMind(other, out _, out _)
            || _mobState.IsDead(other)
            || _mobState.IsCritical(other)
            || TryComp<BlindableComponent>(other, out var blind) && blind.IsBlind
            || TryComp<SlasherIncorporealComponent>(other, out var inc) && inc.IsIncorporeal)
            return false;

        return true;
    }
}
