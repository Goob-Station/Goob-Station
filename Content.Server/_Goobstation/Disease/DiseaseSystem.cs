using Content.Shared.Disease;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Server.Disease;

public sealed partial class DiseaseSystem : SharedDiseaseSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseComponent, DiseaseCloneEvent>(OnClonedInto);
    }

    private void OnClonedInto(EntityUid uid, DiseaseComponent disease, DiseaseCloneEvent args)
    {
        foreach (var effectUid in args.Source.Comp.Effects)
        {
            if (!TryComp<DiseaseEffectComponent>(effectUid, out var effectComp) || !TryComp<MetaDataComponent>(effectUid, out var metadata) || metadata.EntityPrototype == null)
                continue;

            TryAdjustEffect(uid, metadata.EntityPrototype, out _, effectComp.Severity, disease);
        }
        // no idea how to do this better
        disease.InfectionRate = args.Source.Comp.InfectionRate;
        disease.MutationRate = args.Source.Comp.MutationRate;
        disease.ImmunityGainRate = args.Source.Comp.ImmunityGainRate;
        disease.MutationMutationCoefficient = args.Source.Comp.MutationMutationCoefficient;
        disease.ImmunityGainMutationCoefficient = args.Source.Comp.ImmunityGainMutationCoefficient;
        disease.InfectionRateMutationCoefficient = args.Source.Comp.InfectionRateMutationCoefficient;
        disease.ComplexityMutationCoefficient = args.Source.Comp.ComplexityMutationCoefficient;
        disease.SeverityMutationCoefficient = args.Source.Comp.SeverityMutationCoefficient;
        disease.EffectMutationCoefficient = args.Source.Comp.EffectMutationCoefficient;
        disease.GenotypeMutationCoefficient = args.Source.Comp.GenotypeMutationCoefficient;
        disease.Complexity = args.Source.Comp.Complexity;
        disease.Genotype = args.Source.Comp.Genotype;
        disease.CanGainImmunity = args.Source.Comp.CanGainImmunity;
        disease.AffectsDead = args.Source.Comp.AffectsDead;
        disease.DeadInfectionRate = args.Source.Comp.DeadInfectionRate;
        disease.DiseaseType = args.Source.Comp.DiseaseType;
    }

    #region public API

    /// <summary>
    /// Makes a clone of the provided disease entity
    /// </summary>
    public override EntityUid? TryClone(EntityUid source, DiseaseComponent? comp)
    {
        if (!Resolve(source, ref comp))
            return null;

        var ent = Spawn(BaseDisease);
        var ev = new DiseaseCloneEvent((source, comp));
        RaiseLocalEvent(ent, ev);
        return ent;
    }

    /// <summary>
    /// Tries to cure the entity of the given disease entity
    /// </summary>
    public override bool TryCure(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (comp.Diseases.Remove(disease))
            QueueDel(disease);
        else
            return false;

        Dirty(uid, comp);
        return true;
    }

    /// <summary>
    /// Tries to infect the entity with a given disease prototype
    /// </summary>
    public override bool TryInfect(EntityUid uid, EntProtoId diseaseId, [NotNullWhen(true)] out EntityUid? disease, DiseaseCarrierComponent? comp = null, bool force = false)
    {
        disease = null;
        if (!Resolve(uid, ref comp, false))
            return false;

        var spawned = Spawn(diseaseId, new EntityCoordinates(uid, Vector2.Zero));
        if (!TryInfect(uid, spawned, comp, force))
        {
            QueueDel(spawned);
            return false;
        }
        disease = spawned;
        return true;
    }

    #endregion

}
