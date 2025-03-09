using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Shitmed.Surgery.Consciousness;
using Content.Shared._Shitmed.Surgery.Pain.Components;
using Content.Shared.Body.Organ;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Surgery.Pain.Systems;

public partial class PainSystem
{
    private const double PainJobTime = 0.005;
    private readonly JobQueue _painJobQueue = new(PainJobTime);

    #region Public API

    /// <summary>
    /// Changes a pain value for a specific nerve, if there's any. Adds MORE PAIN to it basically.
    /// </summary>
    /// <param name="uid">Uid of the nerveSystem component owner.</param>
    /// <param name="nerveUid">Nerve uid.</param>
    /// <param name="identifier">Identifier of the said modifier.</param>
    /// <param name="change">How many pain to set.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <param name="time">How long will the modifier last?</param>
    /// <returns>Returns true, if PAIN QUOTA WAS COLLECTED.</returns>
    public bool TryChangePainModifier(EntityUid uid, EntityUid nerveUid, string identifier, FixedPoint2 change, NerveSystemComponent? nerveSys = null, TimeSpan? time = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Modifiers.TryGetValue((nerveUid, identifier), out var modifier))
            return false;

        var modifierToSet =
            modifier with {Change = change, Time = time ?? modifier.Time};
        nerveSys.Modifiers[(nerveUid, identifier)] = modifierToSet;

        var ev = new PainModifierChangedEvent(uid, nerveUid, modifier.Change);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Gets a copy of pain modifier.
    /// </summary>
    /// <param name="uid">Uid of the nerveSystem component owner.</param>
    /// <param name="nerveUid">Nerve uid, used to seek for modifier.</param>
    /// <param name="identifier">Identifier of the said modifier.</param>
    /// <param name="modifier">Modifier copy acquired.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if the modifier was acquired.</returns>
    public bool TryGetPainModifier(
        EntityUid uid,
        EntityUid nerveUid,
        string identifier,
        [NotNullWhen(true)] out PainModifier? modifier,
        NerveSystemComponent? nerveSys = null)
    {
        modifier = null;
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Modifiers.TryGetValue((nerveUid, identifier), out var data))
            return false;

        modifier = data;
        return true;
    }

    /// <summary>
    /// Adds pain to needed nerveSystem, uses modifiers.
    /// </summary>
    /// <param name="uid">Uid of the nerveSystem owner.</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage was applied.</param>
    /// <param name="identifier">Identifier of the said modifier.</param>
    /// <param name="change">Number of pain to add.</param>
    /// <param name="nerveSys">NerveSystem component.</param>
    /// <param name="time">Timespan of the modifier's existence</param>
    /// <returns>Returns true, if the PAIN WAS APPLIED.</returns>
    public bool TryAddPainModifier(EntityUid uid, EntityUid nerveUid, string identifier, FixedPoint2 change, NerveSystemComponent? nerveSys = null, TimeSpan? time = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        var modifier = new PainModifier(change, MetaData(nerveUid).EntityPrototype!.ID, _timing.CurTime + time);
        if (!nerveSys.Modifiers.TryAdd((nerveUid, identifier), modifier))
            return false;

        var ev = new PainModifierAddedEvent(uid, nerveUid, change);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Adds a pain feeling modifier to the needed nerve, uses modifiers.
    /// </summary>
    /// <param name="effectOwner">Uid of the owner of this effect.</param>
    /// <param name="identifier">The string identifier of the modifier to add</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage is being applied.</param>
    /// <param name="change">Number of pain feeling to add / subtract.</param>
    /// <param name="nerve">The nerve component of the nerve entity.</param>
    /// <param name="time">The TimeSpan of the effect; When runs out, the effect will be removed.</param>
    /// <returns>Returns true, if the pain feeling modifier was added.</returns>
    public bool TryAddPainFeelsModifier(EntityUid effectOwner, string identifier, EntityUid nerveUid, FixedPoint2 change, NerveComponent? nerve = null, TimeSpan? time = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        var modifier = new PainFeelingModifier(change, _timing.CurTime + time);
        if (!nerve.PainFeelingModifiers.TryAdd((effectOwner, identifier), modifier))
            return false;

        Dirty(nerveUid, nerve);
        return true;
    }

    /// <summary>
    /// Tries to get a pain feeling modifier.
    /// </summary>
    /// <param name="nerveEnt">Uid of the nerve from which you get the modifier.</param>
    /// <param name="effectOwner">Uid of the effect owner.</param>
    /// <param name="identifier">String identifier of the modifier.</param>
    /// <param name="modifier">The modifier you wanted.</param>
    /// <param name="nerve">The nerve component of the nerve entity.</param>
    /// <returns>Returns true, if the pain feeling modifier was added.</returns>
    public bool TryGetPainFeelsModifier(EntityUid nerveEnt,
        EntityUid effectOwner,
        string identifier,
        [NotNullWhen(true)] out PainFeelingModifier? modifier,
        NerveComponent? nerve = null)
    {
        modifier = null;
        if (!Resolve(nerveEnt, ref nerve, false))
            return false;

        if (!nerve.PainFeelingModifiers.TryGetValue((effectOwner, identifier), out var data))
            return false;

        modifier = data;
        return true;
    }

    /// <summary>
    /// Changes a pain feeling modifier of a needed nerve, uses modifiers.
    /// </summary>
    /// <param name="effectOwner">Uid of the owner of this effect.</param>
    /// <param name="identifier">The string identifier of this.. yeah</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage is being applied.</param>
    /// <param name="change">Number of pain feeling to add / subtract.</param>
    /// <param name="nerve">The nerve component of the nerve entity.</param>
    /// <param name="time">The TimeSpan of the effect; When runs out, the effect will be removed.</param>
    /// <returns>Returns true, if the pain feeling modifier was changed.</returns>
    public bool TryChangePainFeelsModifier(
        EntityUid effectOwner,
        string identifier,
        EntityUid nerveUid,
        FixedPoint2 change,
        NerveComponent? nerve = null,
        TimeSpan? time = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        if (!nerve.PainFeelingModifiers.TryGetValue((effectOwner, identifier), out var modifier))
            return false;

        var modifierToSet =
            new PainFeelingModifier(Change: change, Time: time ?? modifier.Time);
        nerve.PainFeelingModifiers[(nerveUid, identifier)] = modifierToSet;

        Dirty(nerveUid, nerve);
        return true;
    }

    /// <summary>
    /// Removes a pain feeling modifier of a needed nerve, uses modifiers.
    /// </summary>
    /// <param name="effectOwner">Uid of the owner of this effect.</param>
    /// <param name="identifier">The identifier of the said modifier.</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage is being applied.</param>
    /// <param name="nerve">The nerve component of the nerve entity.</param>
    /// <returns>Returns true, if the pain feeling modifier was removed.</returns>
    public bool TryRemovePainFeelsModifier(EntityUid effectOwner, string identifier, EntityUid nerveUid, NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        Dirty(nerveUid, nerve);
        return nerve.PainFeelingModifiers.Remove((effectOwner, identifier));
    }

    /// <summary>
    /// Removes a specified pain modifier.
    /// </summary>
    /// <param name="uid">NerveSystemComponent owner.</param>
    /// <param name="nerveUid">Nerve Uid, to which pain is applied.</param>
    /// <param name="identifier">Identifier of the said pain modifier.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if the pain modifier was removed.</returns>
    public bool TryRemovePainModifier(EntityUid uid, EntityUid nerveUid, string identifier, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false) || _net.IsClient)
            return false;

        if (!nerveSys.Modifiers.Remove((nerveUid, identifier)))
            return false;

        var ev = new PainModifierRemovedEvent(uid, nerveUid, nerveSys.Pain);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Adds pain multiplier to nerveSystem.
    /// </summary>
    /// <param name="uid">NerveSystem owner's uid.</param>
    /// <param name="identifier">ID for the multiplier.</param>
    /// <param name="change">Number to multiply.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <param name="time">A timer for this multiplier; Upon it's end, the multiplier gets removed.</param>
    /// <returns>Returns true, if the multiplier was applied.</returns>
    public bool TryAddPainMultiplier(EntityUid uid,
        string identifier,
        FixedPoint2 change,
        NerveSystemComponent? nerveSys = null,
        TimeSpan? time = null)
    {
        if (!Resolve(uid, ref nerveSys, false) || _net.IsClient)
            return false;

        var modifier = new PainMultiplier(change, identifier, _timing.CurTime + time);
        if (!nerveSys.Multipliers.TryAdd(identifier, modifier))
            return false;

        UpdateNerveSystemPain(uid, nerveSys);

        Dirty(uid, nerveSys);
        return true;
    }


    /// <summary>
    /// Changes an existing pain multiplier's data, on a specified nerve system.
    /// </summary>
    /// <param name="uid">NerveSystem owner's uid.</param>
    /// <param name="identifier">ID for the multiplier.</param>
    /// <param name="change">Number to multiply.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if the multiplier was changed.</returns>
    public bool TryChangePainMultiplier(EntityUid uid, string identifier, FixedPoint2 change, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false) || _net.IsClient)
            return false;

        if (!nerveSys.Multipliers.TryGetValue(identifier, out var multiplier))
            return false;

        var multiplierToSet =
            multiplier with {Change = change};
        nerveSys.Multipliers[identifier] = multiplierToSet;

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Removes a pain multiplier.
    /// </summary>
    /// <param name="uid">NerveSystem owner's uid.</param>
    /// <param name="identifier">ID to seek for the multiplier, what must be removed.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if the multiplier was removed.</returns>
    public bool TryRemovePainMultiplier(EntityUid uid, string identifier, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false) || _net.IsClient)
            return false;

        if (!nerveSys.Multipliers.Remove(identifier))
            return false;

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    public Entity<AudioComponent>? PlayPainSound(EntityUid body, NerveSystemComponent nerveSys, SoundSpecifier specifier, AudioParams? audioParams = null)
    {
        var sound = _IHaveNoMouthAndIMustScream.PlayPvs(specifier, body, audioParams);
        if (!sound.HasValue)
            return null;

        nerveSys.PlayedPainSounds.Add(sound.Value.Entity, sound.Value.Component);
        return sound.Value;
    }

    public void PlayPainSound(EntityUid body, NerveSystemComponent nerveSys, SoundSpecifier specifier, TimeSpan delay, AudioParams? audioParams = null)
    {
        nerveSys.PainSoundsToPlay.Add(body, (specifier, audioParams, _timing.CurTime + delay));
    }

    #endregion

    #region Private API

    public sealed class PainTimerJob : Job<object>
    {
        private readonly PainSystem _self;
        private readonly Entity<NerveSystemComponent> _ent;
        public PainTimerJob(PainSystem self, Entity<NerveSystemComponent> ent, double maxTime, CancellationToken cancellation = default) : base(maxTime, cancellation)
        {
            _self = self;
            _ent = ent;
        }

        public PainTimerJob(PainSystem self, Entity<NerveSystemComponent> ent, double maxTime, IStopwatch stopwatch, CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
        {
            _self = self;
            _ent = ent;
        }

        protected override Task<object?> Process()
        {
            _self.UpdateDamage(_ent.Owner, _ent.Comp);
            return Task.FromResult<object?>(null);
        }
    }

    private void UpdateDamage(EntityUid nerveSysEnt, NerveSystemComponent nerveSys)
    {
        if (nerveSys.LastPainThreshold != nerveSys.Pain && _timing.CurTime > nerveSys.UpdateTime)
            nerveSys.LastPainThreshold = nerveSys.Pain;

        foreach (var (key, value) in nerveSys.PainSoundsToPlay)
        {
            if (_timing.CurTime < value.Item3)
                continue;

            PlayPainSound(key, nerveSys, value.Item1, value.Item2);
            nerveSys.PainSoundsToPlay.Remove(key);
        }

        foreach (var (key, value) in nerveSys.Modifiers)
        {
            if (_timing.CurTime > value.Time)
                TryRemovePainModifier(nerveSysEnt, key.Item1, key.Item2, nerveSys);
        }

        foreach (var (key, value) in nerveSys.Multipliers)
        {
            if (_timing.CurTime > value.Time)
                TryRemovePainMultiplier(nerveSysEnt, key, nerveSys);
        }

        // I hate myself.
        foreach (var (ent, nerve) in nerveSys.Nerves)
        {
            foreach (var (key, value) in nerve.PainFeelingModifiers.ToList())
            {
                if (_timing.CurTime > value.Time)
                    TryRemovePainFeelsModifier(key.Item1, key.Item2, ent, nerve);
            }
        }
    }

    private void UpdateNerveSystemPain(EntityUid uid, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return;

        if (!TryComp<OrganComponent>(uid, out var organ) || organ.Body == null)
            return;

        nerveSys.Pain =
            FixedPoint2.Clamp(
                nerveSys.Modifiers.Aggregate((FixedPoint2) 0,
                    (current, modifier) =>
                    current + ApplyModifiersToPain(modifier.Key.Item1, modifier.Value.Change, nerveSys)),
                0,
                nerveSys.PainCap);

        UpdatePainThreshold(uid, nerveSys);
        if (!_consciousness.SetConsciousnessModifier(
                organ.Body.Value,
                uid,
                -nerveSys.Pain,
                identifier: PainModifierIdentifier,
                type: ConsciousnessModType.Pain))
        {
            _consciousness.AddConsciousnessModifier(
                organ.Body.Value,
                uid,
                -nerveSys.Pain,
                identifier: PainModifierIdentifier,
                type: ConsciousnessModType.Pain);
        }
    }

    private void CleanupSounds(NerveSystemComponent nerveSys)
    {
        foreach (var (id, _) in nerveSys.PlayedPainSounds.Where(sound => !TerminatingOrDeleted(sound.Key)))
        {
            _IHaveNoMouthAndIMustScream.Stop(id);
            nerveSys.PlayedPainSounds.Remove(id);
        }

        foreach (var (id, _) in nerveSys.PainSoundsToPlay.Where(sound => !TerminatingOrDeleted(sound.Key)))
        {
            nerveSys.PainSoundsToPlay.Remove(id);
        }
    }

    private void ApplyPainReflexesEffects(EntityUid body, Entity<NerveSystemComponent> nerveSys, PainThresholdTypes reaction)
    {
        if (!_net.IsServer)
            return;

        var sex = Sex.Unsexed;
        if (TryComp<HumanoidAppearanceComponent>(body, out var humanoid))
            sex = humanoid.Sex;

        switch (reaction)
        {
            case PainThresholdTypes.PainFlinch:
                CleanupSounds(nerveSys.Comp);
                PlayPainSound(body, nerveSys.Comp, nerveSys.Comp.PainScreams[sex]);

                _popup.PopupPredicted(Loc.GetString("screams-and-flinches-pain", ("entity", body)), body, null, PopupType.MediumCaution);
                _jitter.DoJitter(body, TimeSpan.FromSeconds(0.9), true, 24f, 1f);

                break;
            case PainThresholdTypes.Agony:
                CleanupSounds(nerveSys);
                PlayPainSound(body, nerveSys, nerveSys.Comp.AgonyScreams[sex], AudioParams.Default.WithVolume(12f));

                // We love violence, don't we?

                _popup.PopupPredicted(Loc.GetString("screams-in-agony", ("entity", body)), body, null, PopupType.MediumCaution);
                _jitter.DoJitter(body, nerveSys.Comp.PainShockStunTime / 1.4, true, 30f, 12f);

                break;
            case PainThresholdTypes.PainShock:
                CleanupSounds(nerveSys);

                var screamSpecifier = nerveSys.Comp.PainShockScreams[sex];
                var scream = PlayPainSound(body, nerveSys, screamSpecifier, AudioParams.Default.WithVolume(12f));
                if (scream.HasValue)
                {
                    var sound = nerveSys.Comp.PainShockWhimpers[sex];
                    PlayPainSound(body,
                        nerveSys,
                        sound,
                        _IHaveNoMouthAndIMustScream.GetAudioLength(_IHaveNoMouthAndIMustScream.GetSound(screamSpecifier)) - TimeSpan.FromSeconds(2),
                        AudioParams.Default.WithVolume(-12f));
                }

                // This shit is NOT helpful. It breaks the multipliers, and every 21 seconds the multiplier ends, you fall into fucking crit
                // and stand up AGAIN due to adrenaline. Thus trapping you in an endless cycle of pain, not funny
                // TryAddPainMultiplier(nerveSys, "PainShockAdrenaline", 0.5f, nerveSys, TimeSpan.FromSeconds(21f));
                PlayPainSound(body, nerveSys, nerveSys.Comp.PainRattles, AudioParams.Default.WithVolume(-6f));

                _popup.PopupPredicted(
                    _standing.IsDown(body)
                        ? Loc.GetString("screams-in-pain", ("entity", body))
                        : Loc.GetString("screams-and-falls-pain", ("entity", body)),
                    body,
                    null,
                    PopupType.MediumCaution);

                _stun.TryParalyze(body, nerveSys.Comp.PainShockStunTime, true);
                _jitter.DoJitter(body, nerveSys.Comp.PainShockStunTime, true, 20f, 7f);

                // For the funnies :3
                _consciousness.ForceConscious(body, nerveSys.Comp.PainShockStunTime);
                nerveSys.Comp.LastThresholdType = PainThresholdTypes.None;

                break;
            case PainThresholdTypes.PainPassout:
                CleanupSounds(nerveSys);

                _popup.PopupPredicted(Loc.GetString("passes-out-pain", ("entity", body)), body, null, PopupType.MediumCaution);
                _consciousness.ForcePassout(body, nerveSys.Comp.ForcePassoutTime);

                nerveSys.Comp.LastThresholdType = PainThresholdTypes.None;

                break;
            case PainThresholdTypes.None:
                break;
        }
    }

    private void UpdatePainThreshold(EntityUid uid, NerveSystemComponent nerveSys)
    {
        var painInput = nerveSys.Pain - nerveSys.LastPainThreshold;

        var nearestReflex = PainThresholdTypes.None;
        foreach (var (reflex, threshold) in nerveSys.PainThresholds.OrderByDescending(kv => kv.Value))
        {
            if (painInput < threshold)
                continue;

            nearestReflex = reflex;
            break;
        }

        if (nearestReflex == PainThresholdTypes.None)
            return;

        if (nerveSys.LastThresholdType == nearestReflex || _timing.CurTime < nerveSys.UpdateTime)
            return;

        if (!TryComp<OrganComponent>(uid, out var organ) || !organ.Body.HasValue)
            return;

        var ev1 = new PainThresholdTriggered(uid, nerveSys, nearestReflex, painInput);
        RaiseLocalEvent(organ.Body.Value, ref ev1);

        if (ev1.Cancelled || _mobState.IsDead(organ.Body.Value))
            return;

        var ev2 = new PainThresholdEffected(uid, nerveSys, nearestReflex, painInput);
        RaiseLocalEvent(organ.Body.Value, ref ev2);

        nerveSys.UpdateTime = _timing.CurTime + nerveSys.ThresholdUpdateTime;
        nerveSys.LastThresholdType = nearestReflex;

        ApplyPainReflexesEffects(organ.Body.Value, (uid, nerveSys), nearestReflex);
    }

    private FixedPoint2 ApplyModifiersToPain(EntityUid nerveUid, FixedPoint2 pain, NerveSystemComponent nerveSys, NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return pain;

        var modifiedPain = pain * nerve.PainMultiplier;
        if (nerveSys.Multipliers.Count == 0)
            return modifiedPain;

        var toMultiply = nerveSys.Multipliers.Sum(multiplier => (int) multiplier.Value.Change);
        return modifiedPain * toMultiply / nerveSys.Multipliers.Count; // o(*^＠^*)o
    }

    #endregion
}
