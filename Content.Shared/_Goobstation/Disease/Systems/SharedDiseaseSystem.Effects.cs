using Content.Shared.Damage;
using Content.Shared.Weapons.Melee;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Disease;

public partial class SharedDiseaseSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    protected virtual void InitializeEffects()
    {
        SubscribeLocalEvent<DiseaseDamageEffectComponent, DiseaseEffectEvent>(OnDamageEffect);
        SubscribeLocalEvent<DiseaseSpreadEffectComponent, DiseaseEffectEvent>(OnDiseaseSpreadEffect);
        SubscribeLocalEvent<DiseaseFightImmunityEffectComponent, DiseaseEffectEvent>(OnFightImmunityEffect);
    }

    private void OnDamageEffect(EntityUid uid, DiseaseDamageEffectComponent effect, DiseaseEffectEvent args)
    {
        _damageable.TryChangeDamage(args.Ent, effect.Damage * GetScale(args, effect), true, false);
    }

    private void OnDiseaseSpreadEffect(EntityUid uid, DiseaseSpreadEffectComponent effect, DiseaseEffectEvent args)
    {
        // for gear that makes you less(/more?) infective to others
        var ev = new DiseaseOutgoingSpreadAttemptEvent(
            effect.InfectionPower,
            effect.InfectionChance,
            effect.SpreadType
        );
        RaiseLocalEvent(args.Ent, ref ev);

        Log.Info($"Trying spread for {ToPrettyString(args.Ent)}");
        if (ev.Power < 0 || ev.Chance < 0)
            return;

        var xform = Transform(args.Ent);
        var (selfPos, selfRot) = _transform.GetWorldPositionRotation(xform);

        var targets = _melee.ArcRayCast(selfPos, selfRot, effect.Arc, effect.Range, xform.MapID, args.Ent);

        foreach (var target in targets)
        {
            Log.Info($"Spreading to {ToPrettyString(target)}");
            // for disease (un)protection gear
            var evIncoming = new DiseaseIncomingSpreadAttemptEvent(
                ev.Power,
                ev.Chance,
                effect.SpreadType
            );
            RaiseLocalEvent(target, ref evIncoming);
            var power = evIncoming.Power;
            var chance = evIncoming.Chance;
            if (power < 0 || chance < 0)
                continue;

            if (_random.Prob(power * chance * GetScale(args, effect)))
                TryInfect(target, args.Disease.Owner);
        }
    }

    private void OnFightImmunityEffect(EntityUid uid, DiseaseFightImmunityEffectComponent effect, DiseaseEffectEvent args)
    {
        ChangeImmunityProgress(args.Disease.Owner, effect.Amount * GetScale(args, effect), args.Disease.Comp);
    }

    protected float GetScale(DiseaseEffectEvent args, ScalingDiseaseEffect effect)
    {
        return (effect.SeverityScale ? args.Severity : 1f)
            * (effect.TimeScale ? (float)args.TimeDelta.TotalSeconds : 1f)
            * (effect.ProgressScale ? args.DiseaseProgress : 1f);
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

        if (!TryComp<DiseaseEffectComponent>(effect, out var effectComp))
        {
            if (spawned)
            {
                TryRemoveEffect(uid, effect.Value.Owner, comp);
            }
            Log.Error($"Attempted to adjust disease effect {effectId}, but it had no DiseaseEffectComponent");
            return false;
        }

        if (spawned)
            effectComp.Severity = 0f;

        effectComp.Severity += delta;
        if (effectComp.Severity <= 0f)
        {
            if (!TryRemoveEffect(uid, effect.Value.Owner, comp))
                return false;
        }

        Dirty(uid, comp);
        return true;
    }

    #endregion
}
