using Content.Goobstation.Shared.Disease.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Disease.Systems;

public partial class SharedDiseaseSystem
{
    /// <summary>
    /// Makes a clone of the provided disease entity
    /// </summary>
    public virtual EntityUid? TryClone(EntityUid source, DiseaseComponent? comp = null)
    {
        // do nothing on client
        return null;
    }

    /// <summary>
    /// Tries to infect the given target with the given disease prototype
    /// </summary>
    public EntityUid? DoInfectionAttempt(EntityUid target, EntProtoId proto, DiseaseSpreadSpecifier spreadParams)
    {
        return DoInfectionAttempt(target, proto, spreadParams.Power, spreadParams.Chance, spreadParams.Type);
    }

    /// <summary>
    /// Tries to infect the given target with the given disease prototype
    /// </summary>
    public virtual EntityUid? DoInfectionAttempt(EntityUid target, EntProtoId proto, float power, float chance, ProtoId<DiseaseSpreadPrototype> spreadType)
    {
        // do nothing on client
        return null;
    }

    public bool DoInfectionAttempt(EntityUid target, EntityUid disease, DiseaseSpreadSpecifier spreadParams, bool clone = true)
    {
        return DoInfectionAttempt(target, disease, spreadParams.Power, spreadParams.Chance, spreadParams.Type, clone);
    }

    /// <summary>
    /// Tries to infect the given target with the given disease, clones and mutates the provided disease by default
    /// </summary>
    public bool DoInfectionAttempt(EntityUid target, EntityUid disease, float power, float chance, ProtoId<DiseaseSpreadPrototype> spreadType, bool clone = true)
    {
        if (!TryComp<DiseaseComponent>(disease, out var diseaseComp))
            return false;

        // prevent the disease mutating a new genotype in-transmission so if you cough at one person many times they can't get infected many times
        if (HasDisease(target, diseaseComp.Genotype))
            return false;

        // for disease (un)protection gear
        var evIncoming = new DiseaseIncomingSpreadAttemptEvent(
            power,
            chance,
            spreadType
        );
        RaiseLocalEvent(target, ref evIncoming);
        power = evIncoming.Power;
        chance = evIncoming.Chance;
        if (power < 0 || chance < 0)
            return false;

        if (_random.Prob(power * chance))
        {
            var infectDisease = disease;
            EntityUid? newDisease = null;
            if (clone)
            {
                newDisease = TryClone(disease, diseaseComp);
                if (newDisease == null)
                    return false;

                MutateDisease(newDisease.Value);
                infectDisease = newDisease.Value;
            }

            bool success = TryInfect(target, infectDisease);
            if (!success && newDisease != null)
                QueueDel(newDisease);

            return success;
        }
        return false;
    }

    /// <summary>
    /// Makes a random disease from a base prototype
    /// By default, will avoid changing anything already present in the base prototype
    /// </summary>
    public virtual EntityUid? MakeRandomDisease(EntProtoId baseProto, float complexity, float mutationChance = 0f)
    {
        // do nothing on client
        return null;
    }

    /// <summary>
    /// Chance function that can take arbitrarily large values
    /// </summary>
    public bool ExpProb(float prob)
    {
        return _random.Prob(1f - MathF.Exp(-prob));
    }

    public const float MinMutationRate = 0.0001f;

    /// <summary>
    /// Mutate the provided disease
    /// Can take an override of mutation rate
    /// </summary>
    public void MutateDisease(EntityUid uid, DiseaseComponent? disease = null, float? mutationRate = null)
    {
        if (!Resolve(uid, ref disease))
            return;

        // if you're reading this and want something to affect mutation rate, make and use an event for it
        float rate = mutationRate ?? disease.MutationRate;

        if (rate < MinMutationRate)
            return;

        // parameter mutation
        disease.MutationRate *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.MutationMutationCoefficient * rate);
        disease.ImmunityGainRate *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.ImmunityGainMutationCoefficient * rate);
        disease.InfectionRate *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.InfectionRateMutationCoefficient * rate);
        // note that this is actually the "target" complexity, actual complexity is brought to this value at the end of this method
        disease.Complexity *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.ComplexityMutationCoefficient * rate);

        // effect mutation
        float effectProb = 1f - MathF.Exp(-disease.EffectMutationCoefficient * rate);
        for (var limit = 0; limit < 20 && _random.Prob(effectProb); limit++) // no infinite loop
        {
            if (_random.Prob(0.5f)) // half chance to remove effect, half chance to add
                RemoveRandomEffect(uid, disease);
            else
                AddRandomEffect(uid, disease);

            effectProb *= effectProb;
        }

        // genotype mutation
        if (ExpProb(disease.GenotypeMutationCoefficient * rate))
            disease.Genotype = _random.Next();

        // effect severity mutation
        foreach (var effectUid in disease.Effects)
        {
            if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                continue;

            if (ExpProb(disease.SeverityMutationCoefficient * rate))
            {
                effect.Severity = _random.NextFloat(effect.MinSeverity, MaxEffectSeverity);
                Dirty(effectUid, effect);
            }
        }

        var complexity = 0f;
        var minComplexity = 0f;
        var maxComplexity = 0f;
        foreach (var effectUid in disease.Effects)
        {
            if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                continue;

            complexity += effect.GetComplexity();
            minComplexity += effect.Complexity * effect.MinSeverity;
            maxComplexity += effect.Complexity * MaxEffectSeverity;
        }
        // try to adjust complexity to target
        // will hopefully succeed on first iteration
        // if it doesn't, whatever the user did, I don't trust it to not infinite loop
        for (var limit = 0; limit < 20; limit++)
        {
            Entity<DiseaseEffectComponent>? changedEffect;
            // we have too many effects for our chosen complexity
            if (disease.Complexity < minComplexity)
            {
                changedEffect = RemoveRandomEffect(uid, disease);
                if (changedEffect != null)
                {
                    complexity -= changedEffect.Value.Comp.GetComplexity();
                    minComplexity -= changedEffect.Value.Comp.Complexity * changedEffect.Value.Comp.MinSeverity;
                    maxComplexity -= changedEffect.Value.Comp.Complexity * MaxEffectSeverity;
                }
            }
            // we have too little effects
            else if (disease.Complexity > maxComplexity)
            {
                changedEffect = AddRandomEffect(uid, disease);
                if (changedEffect != null)
                {
                    complexity += changedEffect.Value.Comp.GetComplexity();
                    minComplexity += changedEffect.Value.Comp.Complexity * changedEffect.Value.Comp.MinSeverity;
                    maxComplexity += changedEffect.Value.Comp.Complexity * MaxEffectSeverity;
                }
            }
            else
            {
                if (disease.Effects.Count < 1)
                {
                    Log.Error($"Disease {ToPrettyString(uid)} tried to mutate effects, but had no effects.");
                    break;
                }
                // by how much we need to adjust complexity
                var delta = disease.Complexity - complexity;

                // try to adjust complexity, adjust severities of random effects until we hit the target
                bool done = false;
                for (var i = 0; i < 20 && !done; i++) // no infinite loops
                {
                    var effectUid = disease.Effects[_random.Next(disease.Effects.Count - 1)];
                    if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                        continue;

                    var targetSeverity = effect.Severity + delta / effect.Complexity;
                    done = targetSeverity > effect.MinSeverity && targetSeverity < MaxEffectSeverity;

                    var oldComplexity = effect.GetComplexity();
                    if (done)
                        // we can bring delta to 0 so do it
                        effect.Severity = targetSeverity;
                    else
                        effect.Severity = delta > 0 ? _random.NextFloat(effect.Severity, MaxEffectSeverity) : _random.NextFloat(effect.MinSeverity, effect.Severity);

                    Dirty(effectUid, effect);

                    // update our current complexity since we updated the effect severity
                    complexity += effect.GetComplexity() - oldComplexity;
                }
                break;
            }
        }

        Dirty(uid, disease);
    }
}
