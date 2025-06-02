// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 PunishedJoe <PunishedJoeseph@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

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
    /// <param name="painType">The damage type of pain</param>
    /// <returns>Returns true, if PAIN QUOTA WAS COLLECTED.</returns>
    public bool TryChangePainModifier(
        EntityUid uid,
        EntityUid nerveUid,
        string identifier,
        FixedPoint2 change,
        NerveSystemComponent? nerveSys = null,
        TimeSpan? time = null,
        PainDamageTypes? painType = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Modifiers.TryGetValue((nerveUid, identifier), out var modifier))
            return false;

        var modifierToSet =
            modifier with { Change = change, Time = _timing.CurTime + time ?? modifier.Time, PainDamageType = painType ?? modifier.PainDamageType };
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
    /// <param name="painType">Damage type for pain</param>
    /// <param name="nerveSys">NerveSystem component.</param>
    /// <param name="time">Timespan of the modifier's existence</param>
    /// <returns>Returns true, if the PAIN WAS APPLIED.</returns>
    public bool TryAddPainModifier(
        EntityUid uid,
        EntityUid nerveUid,
        string identifier,
        FixedPoint2 change,
        PainDamageTypes painType = PainDamageTypes.WoundPain,
        NerveSystemComponent? nerveSys = null,
        TimeSpan? time = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        var modifier = new PainModifier(change, MetaData(nerveUid).EntityPrototype!.ID, painType, _timing.CurTime + time);
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
    public bool TryAddPainFeelsModifier(
        EntityUid effectOwner,
        string identifier,
        EntityUid nerveUid,
        FixedPoint2 change,
        NerveComponent? nerve = null,
        TimeSpan? time = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        var modifier = new PainFeelingModifier(change, _timing.CurTime + time);
        if (!nerve.PainFeelingModifiers.TryAdd((effectOwner, identifier), modifier))
            return false;

        UpdatePainFeels(nerveUid);

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
    /// <returns>Returns true, if the pain feeling modifier was changed.</returns>
    public bool TryChangePainFeelsModifier(
        EntityUid effectOwner,
        string identifier,
        EntityUid nerveUid,
        FixedPoint2 change,
        NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        if (!nerve.PainFeelingModifiers.TryGetValue((effectOwner, identifier), out var modifier))
            return false;

        var modifierToSet =
            modifier with { Change = change };
        nerve.PainFeelingModifiers[(nerveUid, identifier)] = modifierToSet;

        UpdatePainFeels(nerveUid);

        Dirty(nerveUid, nerve);
        return true;
    }

    /// <summary>
    /// Sets a pain feeling modifier of a needed nerve, uses modifiers.
    /// </summary>
    /// <param name="effectOwner">Uid of the owner of this effect.</param>
    /// <param name="identifier">The string identifier of this.. yeah</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage is being applied.</param>
    /// <param name="change">Number of pain feeling to add / subtract.</param>
    /// <param name="nerve">The nerve component of the nerve entity.</param>
    /// <param name="time">The TimeSpan of the effect; When runs out, the effect will be removed.</param>
    /// <returns>Returns true, if the pain feeling modifier was changed.</returns>
    public bool TrySetPainFeelsModifier(
        EntityUid effectOwner,
        string identifier,
        EntityUid nerveUid,
        FixedPoint2 change,
        TimeSpan? time = null,
        NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        if (!nerve.PainFeelingModifiers.TryGetValue((effectOwner, identifier), out var modifier))
            return false;

        var modifierToSet =
            modifier with { Change = change, Time = _timing.CurTime + time ?? modifier.Time };
        nerve.PainFeelingModifiers[(nerveUid, identifier)] = modifierToSet;

        UpdatePainFeels(nerveUid);

        Dirty(nerveUid, nerve);
        return true;
    }

    /// <summary>
    /// Sets a pain feeling modifier of a needed nerve, uses modifiers.
    /// </summary>
    /// <param name="effectOwner">Uid of the owner of this effect.</param>
    /// <param name="identifier">The string identifier of this.. yeah</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage is being applied.</param>
    /// <param name="change">Number of pain feeling to add / subtract.</param>
    /// <param name="nerve">The nerve component of the nerve entity.</param>
    /// <param name="time">The TimeSpan of the effect; When runs out, the effect will be removed.</param>
    /// <returns>Returns true, if the pain feeling modifier was changed.</returns>
    public bool TrySetPainFeelsModifier(
        EntityUid effectOwner,
        string identifier,
        EntityUid nerveUid,
        TimeSpan time,
        NerveComponent? nerve = null,
        FixedPoint2? change = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        if (!nerve.PainFeelingModifiers.TryGetValue((effectOwner, identifier), out var modifier))
            return false;

        var modifierToSet =
            modifier with { Change = change ?? modifier.Change, Time = _timing.CurTime + time };
        nerve.PainFeelingModifiers[(nerveUid, identifier)] = modifierToSet;

        UpdatePainFeels(nerveUid);

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
    public bool TryRemovePainFeelsModifier(
        EntityUid effectOwner,
        string identifier,
        EntityUid nerveUid,
        NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return false;

        nerve.PainFeelingModifiers.Remove((effectOwner, identifier));

        UpdatePainFeels(nerveUid);
        Dirty(nerveUid, nerve);

        return true;
    }

    /// <summary>
    /// Removes a specified pain modifier.
    /// </summary>
    /// <param name="uid">NerveSystemComponent owner.</param>
    /// <param name="nerveUid">Nerve Uid, to which pain is applied.</param>
    /// <param name="identifier">Identifier of the said pain modifier.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if the pain modifier was removed.</returns>
    public bool TryRemovePainModifier(
        EntityUid uid,
        EntityUid nerveUid,
        string identifier,
        NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
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
    /// <param name="painType">Damage type of pain</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <param name="time">A timer for this multiplier; Upon it's end, the multiplier gets removed.</param>
    /// <returns>Returns true, if the multiplier was applied.</returns>
    public bool TryAddPainMultiplier(EntityUid uid,
        string identifier,
        FixedPoint2 change,
        PainDamageTypes painType = PainDamageTypes.WoundPain,
        NerveSystemComponent? nerveSys = null,
        TimeSpan? time = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        var modifier = new PainMultiplier(change, identifier, painType, _timing.CurTime + time);
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
    /// <param name="time">For how long will be this multiplier applied?</param>
    /// <param name="painType">Damage type of pain</param>
    /// <returns>Returns true, if the multiplier was changed.</returns>
    public bool TryChangePainMultiplier(
        EntityUid uid,
        string identifier,
        FixedPoint2 change,
        TimeSpan? time = null,
        PainDamageTypes? painType = null,
        NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Multipliers.TryGetValue(identifier, out var multiplier))
            return false;

        var multiplierToSet =
            multiplier with { Change = change, Time = _timing.CurTime + time ?? multiplier.Time, PainDamageType = painType ?? multiplier.PainDamageType };
        nerveSys.Multipliers[identifier] = multiplierToSet;

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
    /// <param name="time">For how long will be this multiplier applied?</param>
    /// <param name="painType">Damage type of pain</param>
    /// <returns>Returns true, if the multiplier was changed.</returns>
    public bool TryChangePainMultiplier(
        EntityUid uid,
        string identifier,
        TimeSpan time,
        FixedPoint2? change = null,
        PainDamageTypes? painType = null,
        NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Multipliers.TryGetValue(identifier, out var multiplier))
            return false;

        var multiplierToSet =
            multiplier with { Change = change ?? multiplier.Change, Time = _timing.CurTime + time, PainDamageType = painType ?? multiplier.PainDamageType };
        nerveSys.Multipliers[identifier] = multiplierToSet;

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
    /// <param name="time">For how long will be this multiplier applied?</param>
    /// <param name="painType">Damage type of pain</param>
    /// <returns>Returns true, if the multiplier was changed.</returns>
    public bool TryChangePainMultiplier(
        EntityUid uid,
        string identifier,
        PainDamageTypes painType,
        FixedPoint2? change = null,
        TimeSpan? time = null,
        NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Multipliers.TryGetValue(identifier, out var multiplier))
            return false;

        var multiplierToSet =
            multiplier with { Change = change ?? multiplier.Change, Time = _timing.CurTime + time ?? multiplier.Time, PainDamageType = painType };
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
        if (!Resolve(uid, ref nerveSys, false))
            return false;

        if (!nerveSys.Multipliers.Remove(identifier))
            return false;

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    public Entity<AudioComponent>? PlayPainSoundWithCleanup(EntityUid body,
        NerveSystemComponent nerveSys,
        SoundSpecifier specifier,
        AudioParams? audioParams = null,
        string? screamString = null)
    {
        if (!_screamsEnabled
            || !_random.Prob(_screamChance)
            || _mobState.IsDead(body))
            return null;

        CleanupSounds(nerveSys);
        var sound = _IHaveNoMouthAndIMustScream.PlayPvs(specifier, body, audioParams);
        if (!sound.HasValue)
            return null;

        if (screamString != null)
            _popup.PopupPredicted(screamString, body, null, PopupType.MediumCaution);

        nerveSys.PlayedPainSounds.Add(sound.Value.Entity, sound.Value.Component);
        return sound.Value;
    }

    public Entity<AudioComponent>? PlayPainSound(EntityUid body, SoundSpecifier specifier, AudioParams? audioParams = null, string? screamString = null)
    {
        if (!_screamsEnabled
            || !_random.Prob(_screamChance))
            return null;

        if (screamString != null)
            _popup.PopupPredicted(screamString, body, null, PopupType.MediumCaution);

        return _IHaveNoMouthAndIMustScream.PlayPvs(specifier, body, audioParams);
    }

    public Entity<AudioComponent>? PlayPainSound(EntityUid body,
        NerveSystemComponent nerveSys,
        SoundSpecifier specifier,
        AudioParams? audioParams = null,
        string? screamString = null)
    {
        if (!_screamsEnabled
            || !_random.Prob(_screamChance)
            || !TryComp(body, out ConsciousnessComponent? consciousness)
            || !consciousness.HasPainScreams)
            return null;

        var sound = _IHaveNoMouthAndIMustScream.PlayPvs(specifier, body, audioParams);
        if (!sound.HasValue)
            return null;

        if (screamString != null)
            _popup.PopupPredicted(screamString, body, null, PopupType.MediumCaution);

        nerveSys.PlayedPainSounds.Add(sound.Value.Entity, sound.Value.Component);
        return sound.Value;
    }

    public void PlayPainSound(EntityUid body,
        NerveSystemComponent nerveSys,
        SoundSpecifier specifier,
        TimeSpan delay,
        AudioParams? audioParams = null,
        string? screamString = null)
    {
        if (!_screamsEnabled
            || !_random.Prob(_screamChance))
            return;

        if (screamString != null)
            _popup.PopupPredicted(screamString, body, null, PopupType.MediumCaution);

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

    private void UpdatePainFeels(EntityUid nerveUid, NerveComponent? nerveComp = null)
    {
        if (!Resolve(nerveUid, ref nerveComp, false))
            return;

        var bodyPart = Comp<BodyPartComponent>(nerveUid);
        if (bodyPart.Body == null)
            return;

        var ev = new PainFeelsChangedEvent(nerveComp.ParentedNerveSystem, nerveUid, nerveComp.PainFeels);
        RaiseLocalEvent(nerveUid, ref ev);

        if (!TryComp<TargetingComponent>(bodyPart.Body.Value, out var targeting))
            return;

        targeting.BodyStatus = _wound.GetWoundableStatesOnBodyPainFeels(bodyPart.Body.Value);
        Dirty(bodyPart.Body.Value, targeting);

        if (_net.IsServer)
            RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(bodyPart.Body.Value)), bodyPart.Body.Value);
    }

    private void UpdateDamage(EntityUid nerveSysEnt, NerveSystemComponent nerveSys)
    {
        if (!_timing.IsFirstTimePredicted
            || TerminatingOrDeleted(nerveSysEnt)
            || !TryComp<OrganComponent>(nerveSysEnt, out var nerveSysOrgan)
            || nerveSysOrgan.Body is not { } body
            || _mobState.IsDead(body)
            || HasComp<GodmodeComponent>(body))
            return;

        var shouldUpdate = false;
        if (nerveSys.LastPainThreshold != nerveSys.Pain)
        {
            if (_timing.CurTime > nerveSys.UpdateTime)
                nerveSys.LastPainThreshold = nerveSys.Pain;

            if (_timing.CurTime > nerveSys.ReactionUpdateTime)
                UpdatePainThreshold(nerveSysEnt, nerveSys);
            
            shouldUpdate = true;
        }

        if (_timing.CurTime > nerveSys.NextCritScream)
        {
            if (_mobState.IsCritical(body))
            {
                var sex = Sex.Unsexed;
                if (TryComp<HumanoidAppearanceComponent>(body, out var humanoid))
                    sex = humanoid.Sex;

                CleanupSounds(nerveSys);
                if (_trauma.HasBodyTrauma(body, TraumaType.OrganDamage) && _random.Prob(0.22f))
                {
                    // If the person suffers organ damage, do funny gaggling sound :3
                    PlayPainSound(body,
                        nerveSys,
                        nerveSys.OrganDamageWhimpersSounds[sex],
                        AudioParams.Default.WithVolume(-12f));
                }
                else
                {
                    // Play screaming with less chance
                    if (_random.Prob(0.34f))
                        PlayPainSound(body, nerveSys, nerveSys.PainShockScreams[sex], AudioParams.Default.WithVolume(12f));
                    else
                        // Whimpering
                        PlayPainSound(body,
                            nerveSys,                    // Pained or normal
                            _random.Prob(0.34f) ? nerveSys.PainShockWhimpers[sex] : nerveSys.CritWhimpers[sex],
                            AudioParams.Default.WithVolume(-12f));
                }

                nerveSys.NextCritScream = _timing.CurTime + _random.Next(nerveSys.CritScreamsIntervalMin, nerveSys.CritScreamsIntervalMax);
            }
        }

        foreach (var (key, value) in nerveSys.PainSoundsToPlay)
        {
            if (_timing.CurTime < value.Item3)
                continue;

            PlayPainSound(key, nerveSys, value.Item1, value.Item2);
            nerveSys.PainSoundsToPlay.Remove(key);
        }

        foreach (var (key, value) in nerveSys.Modifiers)
            if (_timing.CurTime > value.Time)
                shouldUpdate |= TryRemovePainModifier(nerveSysEnt, key.Item1, key.Item2, nerveSys);

        foreach (var (key, value) in nerveSys.Multipliers)
            if (_timing.CurTime > value.Time)
                shouldUpdate |= TryRemovePainMultiplier(nerveSysEnt, key, nerveSys);

        // I hate myself.
        foreach (var (ent, nerve) in nerveSys.Nerves)
            foreach (var (key, value) in nerve.PainFeelingModifiers.ToList())
                if (_timing.CurTime > value.Time)
                    shouldUpdate |= TryRemovePainFeelsModifier(key.Item1, key.Item2, ent, nerve);

        if (shouldUpdate
            && _net.IsServer)
        {
            RaiseNetworkEvent(new MobThresholdChecked(GetNetEntity(body)), body); // Shitcod to handle overlays.
        }
    }

    private void UpdateNerveSystemPain(EntityUid uid, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys, false)
            || !TryComp<OrganComponent>(uid, out var organ)
            || organ.Body == null)
            return;

        var totalPain = FixedPoint2.Zero;
        var woundPain = FixedPoint2.Zero;

        foreach (var modifier in nerveSys.Modifiers)
        {
            if (modifier.Value.PainDamageType == PainDamageTypes.WoundPain)
                woundPain += ApplyModifiersToPain(modifier.Key.Item1, modifier.Value.Change, nerveSys, modifier.Value.PainDamageType);

            totalPain += ApplyModifiersToPain(modifier.Key.Item1, modifier.Value.Change, nerveSys, modifier.Value.PainDamageType);
        }

        var newPain = FixedPoint2.Clamp(woundPain, 0, nerveSys.SoftPainCap) + totalPain - woundPain;

        nerveSys.UpdateTime = _timing.CurTime + nerveSys.ThresholdUpdateTime;
        if (nerveSys.Pain != newPain)
            nerveSys.ReactionUpdateTime = _timing.CurTime + nerveSys.PainReactionTime;
        nerveSys.Pain = newPain;

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
                var screamString = Loc.GetString("screams-and-flinches-pain", ("entity", body));
                PlayPainSound(body, nerveSys.Comp, nerveSys.Comp.PainScreams[sex], screamString: screamString);

                _jitter.DoJitter(body, TimeSpan.FromSeconds(0.9f), true, 24f, 1f);

                break;
            case PainThresholdTypes.Agony:
                CleanupSounds(nerveSys);
                var agonyString = Loc.GetString("screams-in-agony", ("entity", body));
                PlayPainSound(body, nerveSys, nerveSys.Comp.AgonyScreams[sex], AudioParams.Default.WithVolume(12f), screamString: agonyString);
                _jitter.DoJitter(body, nerveSys.Comp.PainShockStunTime / 1.4, true, 30f, 12f);

                // They aren't put into Pain Sounds, because they aren't supposed to stop after an entity finishes jerking around in pain
                _IHaveNoMouthAndIMustScream.PlayPvs(
                    nerveSys.Comp.PainRattles,
                    body,
                    AudioParams.Default.WithVolume(-12f));

                break;
            case PainThresholdTypes.PainShock:
                CleanupSounds(nerveSys);
                var shockString = _standing.IsDown(body)
                    ? Loc.GetString("screams-in-pain", ("entity", body))
                    : Loc.GetString("screams-and-falls-pain", ("entity", body));
                var screamSpecifier = nerveSys.Comp.PainShockScreams[sex];
                PlayPainSound(body, nerveSys, screamSpecifier, AudioParams.Default.WithVolume(12f), screamString: shockString);

                TryAddPainMultiplier(
                    nerveSys,
                    PainAdrenalineIdentifier,
                    0.7f,
                    PainDamageTypes.WoundPain,
                    nerveSys,
                    nerveSys.Comp.PainShockAdrenalineTime);

                _stun.TryParalyze(body, nerveSys.Comp.PainShockStunTime, true, standOnRemoval: false);
                _jitter.DoJitter(body, nerveSys.Comp.PainShockStunTime, true, 20f, 7f);

                // For the funnies :3
                _consciousness.ForceConscious(body, nerveSys.Comp.PainShockStunTime);

                break;
            case PainThresholdTypes.PainShockAndAgony:
                CleanupSounds(nerveSys);

                var shockAgonyString = _standing.IsDown(body)
                    ? Loc.GetString("screams-in-pain", ("entity", body))
                    : Loc.GetString("screams-and-falls-pain", ("entity", body));
                var agonySpecifier = nerveSys.Comp.AgonyScreams[sex];
                PlayPainSound(body, nerveSys, agonySpecifier, AudioParams.Default.WithVolume(12f), screamString: shockAgonyString);

                _stun.TryParalyze(body, nerveSys.Comp.PainShockStunTime * 1.4, true, standOnRemoval: false);
                _jitter.DoJitter(body, nerveSys.Comp.PainShockStunTime * 1.4, true, 20f, 7f);

                _consciousness.ForceConscious(body, nerveSys.Comp.PainShockStunTime * 1.4);

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

        if (nerveSys.LastThresholdType == nearestReflex && _timing.CurTime < nerveSys.UpdateTime)
            return;

        if (!TryComp<OrganComponent>(uid, out var organ) || !organ.Body.HasValue)
            return;

        var ev1 = new PainThresholdTriggered((uid, nerveSys), nearestReflex, painInput);
        RaiseLocalEvent(organ.Body.Value, ref ev1);

        if (ev1.Cancelled || _mobState.IsDead(organ.Body.Value))
            return;

        var ev2 = new PainThresholdEffected((uid, nerveSys), nearestReflex, painInput);
        RaiseLocalEvent(organ.Body.Value, ref ev2);

        nerveSys.LastThresholdType = nearestReflex;

        ApplyPainReflexesEffects(organ.Body.Value, (uid, nerveSys), nearestReflex);
    }

    private FixedPoint2 ApplyModifiersToPain(
        EntityUid nerveUid,
        FixedPoint2 pain,
        NerveSystemComponent nerveSys,
        PainDamageTypes painType,
        NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve, false))
            return pain;

        var modifiedPain = pain * nerve.PainMultiplier;
        if (nerveSys.Multipliers.Count == 0)
            return modifiedPain;

        var toMultiply =
            nerveSys.Multipliers
                .Where(markiplier => markiplier.Value.PainDamageType == painType)
                .Aggregate(FixedPoint2.Zero, (current, markiplier) => current + markiplier.Value.Change);

        return modifiedPain * toMultiply / nerveSys.Multipliers.Count; // o(*^＠^*)o
    }

    #endregion
}
