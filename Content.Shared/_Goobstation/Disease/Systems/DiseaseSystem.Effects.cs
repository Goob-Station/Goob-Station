using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private void InitializeEffects()
    {
        SubscribeLocalEvent<DiseaseDamageEffectComponent, DiseaseEffectEvent>(OnDamageEffect);
        SubscribeLocalEvent<DiseaseFightImmunityEffectComponent, DiseaseEffectEvent>(OnFightImmunityEffect);
    }

    private void OnDamageEffect(EntityUid uid, DiseaseDamageEffectComponent effect, DiseaseEffectEvent args)
    {
        _damageable.TryChangeDamage(args.Ent, effect.Damage * args.EffectScale, true, false);
    }

    private void OnFightImmunityEffect(EntityUid uid, DiseaseFightImmunityEffectComponent effect, DiseaseEffectEvent args)
    {
        ChangeImmunityProgress(args.Disease.Owner, effect.Amount * args.EffectScale, args.Disease.Comp);
    }

    #region public API

    /// <summary>
    /// Finds an effect of specified prototype, if any
    /// </summary>
    public bool FindEffect(EntityUid uid, EntProtoId effectId, [NotNullWhen(true)] out EntityUid? effect, DiseaseComponent? comp = null)
    {
        effect = null;
        if (!Resolve(uid, ref comp))
            return false;

        var effectProto = _proto.Index(effectId);
        foreach (var effectUid in comp.Effects)
        {
            if (effectProto == Prototype(effectUid))
            {
                effect = effectUid;
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
    /// Removes the disease effect of specified prototype from this disease
    /// </summary>
    public bool TryRemoveEffect(EntityUid uid, EntProtoId effectId, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || !FindEffect(uid, effectId, out var effect, comp))
            return false;

        if (comp.Effects.Remove(effect.Value))
            QueueDel(effect);
        else
            return false;

        Dirty(uid, comp);
        return true;
    }

    /// <summary>
    /// Adds an effect of given prototype to the specified disease
    /// </summary>
    public bool TryAddEffect(EntityUid uid, EntProtoId effectId, [NotNullWhen(true)] out EntityUid? effect, DiseaseComponent? comp = null)
    {
        effect = null;
        if (_net.IsClient || !Resolve(uid, ref comp) || HasEffect(uid, effectId, comp))
            return false;

        effect = Spawn(effectId);
        comp.Effects.Add(effect.Value);
        return true;
    }

    /// <summary>
    /// Tries to adjust the strength of the effect of given prototype, creating or removing it as needed
    /// Non-present effects are assumed to have severity 0 regardless of the prototype's specified severity
    /// </summary>
    public bool TryAdjustEffect(EntityUid uid, EntProtoId effectId, float delta = 1f, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        bool spawned = false;
        FindEffect(uid, effectId, out var effect, comp);
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
                comp.Effects.Remove(effect.Value);
                QueueDel(effect);
            }
            Log.Error($"Attempted to adjust disease effect {effectId}, but it had no DiseaseEffectComponent");
            return false;
        }

        if (spawned)
            effectComp.Severity = 0f;

        effectComp.Severity += delta;
        if (effectComp.Severity <= 0f)
        {
            if (!TryRemoveEffect(uid, effectId, comp))
                return false;
        }

        Dirty(uid, comp);
        return true;
    }

    #endregion
}
