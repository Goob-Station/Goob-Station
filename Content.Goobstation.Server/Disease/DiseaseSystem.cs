using Content.Shared.Rejuvenate;
using Content.Goobstation.Shared.Disease;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
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

    private void OnClonedInto(EntityUid uid, DiseaseComponent disease, DiseaseCloneEvent args)
    {
        foreach (var effectUid in args.Source.Comp.Effects)
        {
            if (!TryComp<DiseaseEffectComponent>(effectUid, out var effectComp) || MetaData(effectUid).EntityPrototype == null)
                continue;

            var entProtoId = MetaData(effectUid).EntityPrototype;
            if (entProtoId != null)
                TryAdjustEffect(uid, entProtoId, out _, effectComp.Severity, disease);
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
        disease.AvailableEffects = args.Source.Comp.AvailableEffects;
        disease.DiseaseType = args.Source.Comp.DiseaseType;
    }

    private void OnGrantDiseaseInit(EntityUid uid, GrantDiseaseComponent grant, MapInitEvent args)
    {
        var disease = MakeRandomDisease(grant.BaseDisease, grant.Complexity);
        if (TryComp<DiseaseComponent>(disease, out var diseaseComp))
            diseaseComp.InfectionProgress = grant.Severity;
        if (disease == null)
            return;
        if (!TryInfect(uid, disease.Value))
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

    private void OnRejuvenate(EntityUid uid, DiseaseCarrierComponent comp, RejuvenateEvent args)
    {
        while (comp.Diseases.Count != 0)
        {
            if (!TryCure(uid, comp.Diseases[0], comp))
                break;
        }
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
        // requiring us to add DiseaseCarrier is just inconveniencing the user for no reason
        EnsureComp<DiseaseComponent>(ent, out var disease);
        disease.Complexity = complexity;
        disease.Genotype = _random.Next();
        MutateDisease(ent, disease, mutationRate);
        return ent;
    }

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
    protected override bool TryCure(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null)
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

        if (force)
            EnsureComp<DiseaseCarrierComponent>(uid, out comp);

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
