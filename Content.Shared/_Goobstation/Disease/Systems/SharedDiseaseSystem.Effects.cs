using Content.Shared.Damage;
using Content.Shared.Flash;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Disease;

public partial class SharedDiseaseSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedFlashSystem _flash = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public float MaxEffectSeverity = 1f; // magic numbers are EVIL and BAD

    protected List<EntityPrototype> _diseaseEffects = new();

    protected virtual void InitializeEffects()
    {
        SubscribeLocalEvent<DiseaseAudioEffectComponent, DiseaseEffectEvent>(OnAudioEffect);
        SubscribeLocalEvent<DiseaseDamageEffectComponent, DiseaseEffectEvent>(OnDamageEffect);
        SubscribeLocalEvent<DiseaseSpreadEffectComponent, DiseaseEffectEvent>(OnDiseaseSpreadEffect);
        SubscribeLocalEvent<DiseaseFightImmunityEffectComponent, DiseaseEffectEvent>(OnFightImmunityEffect);
        SubscribeLocalEvent<DiseaseFlashEffectComponent, DiseaseEffectEvent>(OnFlashEffect);
        SubscribeLocalEvent<DiseasePopupEffectComponent, DiseaseEffectEvent>(OnPopupEffect);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        LoadPrototypes();
    }

    private void OnAudioEffect(EntityUid uid, DiseaseAudioEffectComponent effect, DiseaseEffectEvent args)
    {
        if (_net.IsClient)
            return;

        var sound = effect.Sound;
        if (effect.SoundFemale != null && TryComp<HumanoidAppearanceComponent>(args.Ent, out var humanoid) && humanoid.Sex == Sex.Female)
            sound = effect.SoundFemale;

        _audio.PlayPvs(sound, args.Ent);
    }

    private void OnDamageEffect(EntityUid uid, DiseaseDamageEffectComponent effect, DiseaseEffectEvent args)
    {
        if (_timing.IsFirstTimePredicted)
            _damageable.TryChangeDamage(args.Ent, effect.Damage * GetScale(args, effect), true, false);
    }

    private void OnDiseaseSpreadEffect(EntityUid uid, DiseaseSpreadEffectComponent effect, DiseaseEffectEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        // for gear that makes you less(/more?) infective to others
        var ev = new DiseaseOutgoingSpreadAttemptEvent(
            effect.InfectionPower,
            effect.InfectionChance,
            effect.SpreadType
        );
        RaiseLocalEvent(args.Ent, ref ev);

        if (ev.Power < 0 || ev.Chance < 0)
            return;

        var xform = Transform(args.Ent);
        var (selfPos, selfRot) = _transform.GetWorldPositionRotation(xform);

        var targets = _melee.ArcRayCast(selfPos, selfRot, effect.Arc, effect.Range, xform.MapID, args.Ent);

        foreach (var target in targets)
        {
            DoInfectionAttempt(target, args.Disease, ev.Power, ev.Chance * GetScale(args, effect), effect.SpreadType);
        }
    }

    private void OnFightImmunityEffect(EntityUid uid, DiseaseFightImmunityEffectComponent effect, DiseaseEffectEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        ChangeImmunityProgress(args.Disease.Owner, effect.Amount * GetScale(args, effect), args.Disease.Comp);
    }

    private void OnFlashEffect(EntityUid uid, DiseaseFlashEffectComponent effect, DiseaseEffectEvent args)
    {
        if (_net.IsClient) // flashes twice if ran on both server and client
            return;

        _status.TryAddStatusEffect<FlashedComponent>(args.Ent, _flash.FlashedKey, effect.Duration * GetScale(args, effect), true);
        _stun.TrySlowdown(args.Ent, effect.Duration * GetScale(args, effect), true, effect.SlowTo, effect.SlowTo);

        if (effect.StunDuration != null)
            _stun.TryKnockdown(args.Ent, effect.StunDuration.Value * GetScale(args, effect), true);
    }

    private void OnPopupEffect(EntityUid uid, DiseasePopupEffectComponent effect, DiseaseEffectEvent args)
    {
        if (_net.IsClient)
            return;

        if (effect.HostOnly)
            _popup.PopupEntity(Loc.GetString(effect.String, ("source", args.Ent)), args.Ent, args.Ent, effect.Type);
        else
            _popup.PopupEntity(Loc.GetString(effect.String, ("source", args.Ent)), args.Ent, effect.Type);
    }

    protected float GetScale(DiseaseEffectEvent args, ScalingDiseaseEffect effect)
    {
        return (effect.SeverityScale ? args.Comp.Severity : 1f)
            * (effect.TimeScale ? (float)UpdateInterval.TotalSeconds : 1f)
            * (effect.ProgressScale ? args.Disease.Comp.InfectionProgress : 1f);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>())
            return;

        LoadPrototypes();
    }

    private void LoadPrototypes()
    {
        _diseaseEffects.Clear();

        foreach (var entProto in _proto.EnumeratePrototypes<EntityPrototype>())
            if (entProto.HasComponent<DiseaseEffectComponent>())
                _diseaseEffects.Add(entProto);
    }

    private Entity<DiseaseEffectComponent>? RemoveRandomEffect(EntityUid uid, DiseaseComponent disease)
    {
        if (disease.Effects.Count < 1)
        {
            Log.Error($"Disease {ToPrettyString(uid)} attempted to remove a random effect, but had no effects left.");
            return null;
        }
        var index = _random.Next(disease.Effects.Count - 1);
        var effectUid = disease.Effects[index];
        disease.Effects.RemoveAt(index);

        Dirty(uid, disease);
        return TryComp<DiseaseEffectComponent>(effectUid, out var comp) ? (effectUid, comp) : null;
    }

    private Entity<DiseaseEffectComponent>? AddRandomEffect(EntityUid uid, DiseaseComponent disease)
    {
        List<EntityPrototype> valid = new();
        foreach (var effectProto in _diseaseEffects)
        {
            if (effectProto.TryGetComponent<DiseaseEffectComponent>(out var effectProtoComp, _factory)
                && effectProtoComp.AllowedDiseaseTypes.Contains(disease.DiseaseType)
                && !HasEffect(uid, effectProto.ID, disease))
                valid.Add(effectProto);
        }
        if (valid.Count == 0)
        {
            Log.Error($"Disease {ToPrettyString(uid)} attempted to mutate to add an effect, but there are no valid effects for its type.");
            return null;
        }
        var proto = valid[_random.Next(valid.Count - 1)];
        Entity<DiseaseEffectComponent>? effect = null;
        if (proto.TryGetComponent<DiseaseEffectComponent>(out var effectComp, _factory))
            TryAdjustEffect(uid, proto, out effect, _random.NextFloat(effectComp.MinSeverity, 1f), disease);

        Dirty(uid, disease);
        return effect;
    }

    #region public API

    /// <summary>
    /// Finds an effect of specified prototype, if any
    /// </summary>
    public bool FindEffect(EntityUid uid, EntProtoId effectId, [NotNullWhen(true)] out Entity<DiseaseEffectComponent>? outEffect, DiseaseComponent? comp = null)
    {
        outEffect = null;
        if (!Resolve(uid, ref comp))
            return false;

        var effectProto = _proto.Index(effectId);
        foreach (var effectUid in comp.Effects)
        {
            if (effectProto == Prototype(effectUid))
            {
                if (!TryComp<DiseaseEffectComponent>(effectUid, out var diseaseEffect))
                {
                    Log.Error($"Found disease effect {ToPrettyString(effectUid)} without DiseaseEffectComponent");
                    return false;
                }
                outEffect = (effectUid, diseaseEffect);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the disease has an effect of specified prototype
    /// </summary>
    public bool HasEffect(EntityUid uid, EntProtoId effectId, DiseaseComponent? comp = null)
    {
        return FindEffect(uid, effectId, out _, comp);
    }

    /// <summary>
    /// Removes the specified disease effect from this disease
    /// </summary>
    public virtual bool TryRemoveEffect(EntityUid uid, EntityUid effect, DiseaseComponent? comp = null)
    {
        // does nothing on client
        return false;
    }

    /// <summary>
    /// Removes the disease effect of specified prototype from this disease
    /// </summary>
    public bool TryRemoveEffect(EntityUid uid, EntProtoId effectId, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || !FindEffect(uid, effectId, out var effect, comp))
            return false;

        return TryRemoveEffect(uid, effect.Value, comp);
    }

    /// <summary>
    /// Removes the specified disease effect from this disease
    /// </summary>
    public virtual bool TryAddEffect(EntityUid uid, EntityUid effectUid, [NotNullWhen(true)] out Entity<DiseaseEffectComponent>? effect, DiseaseComponent? comp = null)
    {
        effect = null;
        if (!Resolve(uid, ref comp, false))
            return false;

        if (!TryComp<DiseaseEffectComponent>(effectUid, out var diseaseEffect))
        {
            Log.Error($"Tried to add disease effect {ToPrettyString(effect)}, but it had no DiseaseEffectComponent");
            return false;
        }
        effect = (effectUid, diseaseEffect);
        comp.Effects.Add(effectUid);

        Dirty(uid, comp);
        return true;
    }

    /// <summary>
    /// Adds an effect of given prototype to the specified disease
    /// </summary>
    public virtual bool TryAddEffect(EntityUid uid, EntProtoId effectId, [NotNullWhen(true)] out Entity<DiseaseEffectComponent>? effect, DiseaseComponent? comp = null)
    {
        effect = null;
        // does nothing on client
        return false;
    }

    /// <summary>
    /// Tries to adjust the strength of the effect of given prototype, creating or removing it as needed
    /// Non-present effects are assumed to have severity 0 regardless of the prototype's specified severity
    /// </summary>
    public bool TryAdjustEffect(EntityUid uid, EntProtoId effectId, [NotNullWhen(true)] out Entity<DiseaseEffectComponent>? effect, float delta, DiseaseComponent? comp = null)
    {
        effect = null;
        if (!Resolve(uid, ref comp))
            return false;

        bool spawned = false;
        FindEffect(uid, effectId, out effect, comp);
        if (effect == null)
        {
            spawned = true;
            if (!TryAddEffect(uid, effectId, out effect, comp))
                return false;
        }

        if (spawned)
            effect.Value.Comp.Severity = 0f;

        effect.Value.Comp.Severity += delta;
        if (effect.Value.Comp.Severity <= 0f)
        {
            if (!TryRemoveEffect(uid, effect.Value.Owner, comp))
                return false;
        }

        Dirty(effect.Value);
        Dirty(uid, comp);
        return true;
    }

    #endregion
}
