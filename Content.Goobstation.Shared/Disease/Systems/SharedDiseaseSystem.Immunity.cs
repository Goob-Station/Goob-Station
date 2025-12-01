using Content.Goobstation.Shared.Disease.Components;

namespace Content.Goobstation.Shared.Disease.Systems;

public partial class SharedDiseaseSystem
{
    private void InitializeImmunity()
    {
        SubscribeLocalEvent<ImmunityComponent, GetImmunityEvent>(OnGetImmunity);
        SubscribeLocalEvent<ImmunityComponent, DiseaseGainedEvent>(OnImmunityDiseaseGained);
        SubscribeLocalEvent<ImmunityComponent, DiseaseInfectAttemptEvent>(OnImmunityInfectAttempt);
    }

    private void OnGetImmunity(EntityUid uid, ImmunityComponent immunity, ref GetImmunityEvent args)
    {
        args.ImmunityGainRate += immunity.ImmunityGainRate;
        args.ImmunityStrength += immunity.ImmunityStrength;
    }

    private void OnImmunityDiseaseGained(EntityUid uid, ImmunityComponent immunity, DiseaseGainedEvent args)
    {
        if (!args.DiseaseGained.Comp.CanGainImmunity)
            return;

        TryAddImmunity(uid, args.DiseaseGained.Owner, immunity, args.DiseaseGained.Comp);
    }

    private void OnImmunityInfectAttempt(EntityUid uid, ImmunityComponent immunity, DiseaseInfectAttemptEvent args)
    {
        if (HasImmunity(uid, args.Disease.Owner, immunity, args.Disease.Comp))
            args.CanInfect = false;
    }

    #region public API

    /// <summary>
    /// Checks whether the entity has developed an immunity to this genotype
    /// </summary>
    public bool HasImmunity(EntityUid uid, int genotype, ImmunityComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        return comp.ImmuneTo.Contains(genotype);
    }

    /// <summary>
    /// Checks whether the entity has developed an immunity to this disease
    /// </summary>
    public bool HasImmunity(EntityUid uid, EntityUid disease, ImmunityComponent? comp = null, DiseaseComponent? diseaseComp = null)
    {
        if (!Resolve(disease, ref diseaseComp))
            return false;

        return HasImmunity(uid, diseaseComp.Genotype, comp);
    }

    /// <summary>
    /// Checks whether this entity can be infected by diseases of this genotype
    /// </summary>
    public bool CanInfect(EntityUid uid, int genotype, DiseaseCarrierComponent? comp = null, ImmunityComponent? immunityComp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        return !HasDisease(uid, genotype, comp) && !HasImmunity(uid, genotype, immunityComp);
    }

    public bool TryAddImmunity(EntityUid uid, int genotype, ImmunityComponent? immunity)
    {
        if (!Resolve(uid, ref immunity, false))
            return false;

        immunity.ImmuneTo.Add(genotype);
        return true;
    }

    public bool TryAddImmunity(EntityUid uid, EntityUid disease, ImmunityComponent? immunity = null, DiseaseComponent? diseaseComp = null)
    {
        if (!Resolve(disease, ref diseaseComp))
            return false;

        return TryAddImmunity(uid, diseaseComp.Genotype, immunity);
    }

    public bool CanImmunityAffect(EntityUid uid, DiseaseComponent disease, ImmunityComponent? immunity = null)
    {
        if (!Resolve(uid, ref immunity, false))
            return false;

        return CanImmunityAffect((uid, immunity), disease);
    }

    public bool CanImmunityAffect(Entity<ImmunityComponent> immunity, DiseaseComponent disease)
    {
        if (_mobState.IsDead(immunity.Owner) && !immunity.Comp.InDead)
            return false;

        return immunity.Comp.AffectedTypes.Contains(disease.DiseaseType);
    }

    #endregion
}
