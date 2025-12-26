using Content.Shared.Rejuvenate;
using Content.Goobstation.Shared.Disease;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Disease.Components;
using Content.Goobstation.Shared.Disease.Systems;

namespace Content.Goobstation.Server.Disease;

public sealed partial class DiseaseSystem : SharedDiseaseSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseComponent, DiseaseCloneEvent>(OnClonedInto);
        SubscribeLocalEvent<GrantDiseaseComponent, MapInitEvent>(OnGrantDiseaseInit);
        // SubscribeLocalEvent<InternalsComponent, DiseaseIncomingSpreadAttemptEvent>(OnInternalsIncomingSpread); // TODO: fix
        SubscribeLocalEvent<DiseaseCarrierComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnClonedInto(Entity<DiseaseComponent> ent, ref DiseaseCloneEvent args)
    {
        foreach (var effectUid in args.Source.Comp.Effects.ContainedEntities)
        {
            if (!EffectQuery.TryComp(effectUid, out var effectComp) || MetaData(effectUid).EntityPrototype == null)
                continue;

            var entProtoId = MetaData(effectUid).EntityPrototype;
            if (entProtoId != null)
                TryAdjustEffect((ent, ent.Comp), entProtoId, out _, effectComp.Severity);
        }

        ent.Comp.InfectionRate = args.Source.Comp.InfectionRate;
        ent.Comp.MutationRate = args.Source.Comp.MutationRate;
        ent.Comp.ImmunityGainRate = args.Source.Comp.ImmunityGainRate;
        ent.Comp.MutationMutationCoefficient = args.Source.Comp.MutationMutationCoefficient;
        ent.Comp.ImmunityGainMutationCoefficient = args.Source.Comp.ImmunityGainMutationCoefficient;
        ent.Comp.InfectionRateMutationCoefficient = args.Source.Comp.InfectionRateMutationCoefficient;
        ent.Comp.ComplexityMutationCoefficient = args.Source.Comp.ComplexityMutationCoefficient;
        ent.Comp.SeverityMutationCoefficient = args.Source.Comp.SeverityMutationCoefficient;
        ent.Comp.EffectMutationCoefficient = args.Source.Comp.EffectMutationCoefficient;
        ent.Comp.GenotypeMutationCoefficient = args.Source.Comp.GenotypeMutationCoefficient;
        ent.Comp.Complexity = args.Source.Comp.Complexity;
        ent.Comp.Genotype = args.Source.Comp.Genotype;
        ent.Comp.CanGainImmunity = args.Source.Comp.CanGainImmunity;
        ent.Comp.AffectsDead = args.Source.Comp.AffectsDead;
        ent.Comp.DeadInfectionRate = args.Source.Comp.DeadInfectionRate;
        ent.Comp.AvailableEffects = args.Source.Comp.AvailableEffects;
        ent.Comp.DiseaseType = args.Source.Comp.DiseaseType;
    }

    private void OnGrantDiseaseInit(Entity<GrantDiseaseComponent> ent, ref MapInitEvent args)
    {
        var disease = MakeRandomDisease(ent.Comp.BaseDisease, ent.Comp.Complexity);

        if (disease == null)
            return;

        if (TryComp<DiseaseComponent>(disease, out var diseaseComp))
        {
            if (ent.Comp.PossibleTypes != null)
                diseaseComp.DiseaseType = _random.Pick(ent.Comp.PossibleTypes);

            diseaseComp.InfectionProgress = ent.Comp.Severity;
        }

        if (!TryInfect(ent.Owner, disease.Value))
            QueueDel(disease);
    }

    /* TODO: fix
    private void OnInternalsIncomingSpread(EntityUid uid, InternalsComponent internals, DiseaseIncomingSpreadAttemptEvent args)
    {
        if (_proto.TryIndex<DiseaseSpreadPrototype>(args.Type, out var spreadProto) && _internals.AreInternalsWorking(uid, internals))
        {
            args.ApplyModifier(internals.IncomingInfectionModifier);
        }
    }
    */

    private void OnRejuvenate(Entity<DiseaseCarrierComponent> ent, ref RejuvenateEvent args)
    {
        TryCureAll((ent, ent.Comp));
    }

    #region public API

    /// <summary>
    /// Tries to infect the given target with the given disease prototype
    /// </summary>
    public override EntityUid? DoInfectionAttempt(EntityUid target, EntProtoId proto, float power, float chance, ProtoId<DiseaseSpreadPrototype> spreadType)
    {
        var ent = Spawn(proto);
        if (DoInfectionAttempt(target, ent, power, chance, spreadType, false))
            return ent;

        QueueDel(ent);
        return null;
    }

    /// <summary>
    /// Makes a random disease from a base prototype
    /// By default, will avoid changing anything already present in the base prototype
    /// </summary>
    public override EntityUid? MakeRandomDisease(EntProtoId baseProto, float complexity, float mutationRate = 0f)
    {
        var ent = Spawn(baseProto);
        EnsureComp<DiseaseComponent>(ent, out var disease);
        disease.Complexity = complexity;
        disease.Genotype = _random.Next();
        MutateDisease(ent);
        return ent;
    }

    /// <summary>
    /// Makes a clone of the provided disease entity
    /// </summary>
    public override EntityUid? TryClone(Entity<DiseaseComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return null;

        var disease = Spawn(BaseDisease);
        var ev = new DiseaseCloneEvent((ent, ent.Comp));
        RaiseLocalEvent(disease, ref ev);
        return disease;
    }

    /// <summary>
    /// Tries to cure the entity of the given disease entity
    /// </summary>
    public override bool TryCure(Entity<DiseaseCarrierComponent?> ent, EntityUid disease)
    {
        if (!Resolve(ent, ref ent.Comp) || !ent.Comp.Diseases.Contains(disease))
            return false;

        if (TryComp<DiseaseComponent>(disease, out var diseaseComp))
        {
            foreach (var effect in diseaseComp.Effects.ContainedEntities)
                CleanupEffect((disease, diseaseComp), effect);
        }

        QueueDel(disease);
        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Tries to cure the entity of all diseases
    /// </summary>
    public override bool TryCureAll(Entity<DiseaseCarrierComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        while (ent.Comp.Diseases.Count != 0)
        {
            if (!TryCure((ent, ent.Comp), ent.Comp.Diseases.ContainedEntities[0]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Tries to infect the entity with a given disease prototype
    /// </summary>
    public override bool TryInfect(Entity<DiseaseCarrierComponent?> ent, EntProtoId diseaseId, [NotNullWhen(true)] out EntityUid? disease, bool force = false)
    {
        disease = null;

        if (force)
            EnsureComp<DiseaseCarrierComponent>(ent, out ent.Comp);

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        var spawned = Spawn(diseaseId, new EntityCoordinates(ent, Vector2.Zero));
        if (!TryInfect(ent, spawned, force))
        {
            QueueDel(spawned);
            return false;
        }
        disease = spawned;
        return true;
    }

    #endregion

}
