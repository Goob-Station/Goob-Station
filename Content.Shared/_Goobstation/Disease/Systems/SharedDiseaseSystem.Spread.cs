using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System;

namespace Content.Shared.Disease;

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
    /// Tries to infect the given target with the given disease, clones and mutates the provided disease by default
    /// </summary>
    public void DoInfectionAttempt(EntityUid target, EntityUid disease, float power, float chance, ProtoId<DiseaseSpreadPrototype> spreadType, bool clone = true)
    {
        if (!TryComp<DiseaseComponent>(disease, out var diseaseComp))
            return;

        // prevent the disease mutating a new genotype in-transmission so if you cough at one person many times they can't get infected many times
        if (HasDisease(target, diseaseComp.Genotype))
            return;

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
            return;

        if (_random.Prob(power * chance))
        {
            var infectDisease = disease;
            EntityUid? newDisease = null;
            if (clone)
            {
                newDisease = TryClone(disease, diseaseComp);
                if (newDisease == null)
                    return;

                MutateDisease(newDisease.Value);
                infectDisease = newDisease.Value;
            }

            if (!TryInfect(target, infectDisease) && newDisease != null)
                QueueDel(newDisease);
        }
    }

    public void MutateDisease(EntityUid uid, DiseaseComponent? disease = null)
    {
        if (!Resolve(uid, ref disease))
            return;

        // if you're reading this and want something to affect mutation rate, make and use an event for it
        float rate = disease.MutationRate;
        // parameter mutation
        disease.MutationRate *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.MutationMutationCoefficient * rate);
        disease.ImmunityGainRate *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.ImmunityGainMutationCoefficient * rate);
        disease.InfectionRate *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.InfectionRateMutationCoefficient * rate);
        // note that this is actually the "target" complexity, actual complexity is brought to this value at the end of this method
        disease.Complexity *= MathF.Exp(_random.NextFloat(-1f, 1f) * disease.ComplexityMutationCoefficient * rate);

        // effect mutation
        if (_random.Prob(1f - MathF.Exp(-disease.EffectMutationCoefficient * rate)))
        {
            if (_random.Prob(0.5f)) // half chance to remove effect, half chance to add
                RemoveRandomEffect(uid, disease);
            else
                AddRandomEffect(uid, disease);
        }

        // genotype mutation
        if (_random.Prob(1f - MathF.Exp(-disease.GenotypeMutationCoefficient * rate)))
            disease.Genotype = _random.Next();

        // effect severity mutation
        foreach (var effectUid in disease.Effects)
        {
            if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                continue;

            if (_random.Prob(1f - MathF.Exp(-disease.SeverityMutationCoefficient * rate)))
            {
                effect.Severity = _random.NextFloat(effect.MinSeverity, MaxEffectSeverity);
                Dirty(effectUid, effect);
            }
        }

        // try to adjust complexity to target
        // will hopefully succeed on first iteration
        // if it doesn't, whatever the user did, i don't trust it to not infinite loop
        for (var limit = 0; limit < 20; limit++)
        {
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
            // we have too many effects for our chosen complexity
            if (disease.Complexity < minComplexity)
                RemoveRandomEffect(uid, disease);
            // we have too little effects
            else if (disease.Complexity > maxComplexity)
                AddRandomEffect(uid, disease);
            else
            {
                if (disease.Effects.Count < 1)
                {
                    Log.Error($"Disease {ToPrettyString(uid)} tried to mutate effects, but had no effects.");
                    return;
                }
                // by how much we need to adjust complexity
                var delta = disease.Complexity - complexity;

                // try to decrease complexity, adjust severities of random effects until we hit the target
                if (delta < 0)
                {
                    bool done = false;
                    for (var i = 0; i < 20 && !done; i++) // no infinite loops
                    {
                        var effectUid = disease.Effects[_random.Next(disease.Effects.Count - 1)];
                        if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                            return;

                        var maxChange = effect.Complexity * effect.MinSeverity - effect.GetComplexity(); // maximum amount we can change the complexity by
                        var targetSeverity = effect.Severity + delta / effect.Complexity;
                        done = targetSeverity > effect.MinSeverity && targetSeverity < MaxEffectSeverity;

                        var oldComplexity = effect.GetComplexity();
                        if (done)
                            // we can bring delta to 0 so do it
                            effect.Severity = targetSeverity;
                        else
                            effect.Severity = _random.NextFloat(effect.MinSeverity, MaxEffectSeverity);

                        Dirty(effectUid, effect);

                        // update our current complexity since we updated the effect severity
                        complexity += effect.GetComplexity() - oldComplexity;
                    }
                }
                else
                {
                    // same as above but try to increase complexity for deltas > 0
                    bool done = false;
                    for (var i = 0; i < 20 && !done; i++)
                    {
                        var effectUid = disease.Effects[_random.Next(disease.Effects.Count - 1)];
                        if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                            return;

                        var maxChange = effect.Complexity * MaxEffectSeverity - effect.GetComplexity();
                        var targetSeverity = effect.Severity + delta / effect.Complexity;
                        done = targetSeverity > effect.MinSeverity && targetSeverity < MaxEffectSeverity;

                        var oldComplexity = effect.GetComplexity();
                        if (done)
                            effect.Severity = targetSeverity;
                        else
                            effect.Severity = _random.NextFloat(effect.MinSeverity, MaxEffectSeverity);

                        Dirty(effectUid, effect);

                        complexity += effect.GetComplexity() - oldComplexity;
                    }
                }
                break;
            }
        }

        Dirty(uid, disease);
    }
}
