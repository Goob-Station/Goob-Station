using Content.Shared.Disease;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Disease;

public abstract partial class SharedDiseaseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private TimeSpan _accumulator = TimeSpan.FromSeconds(0);

    /// <summary>
    /// The interval between updates of disease and disease effect entities
    /// </summary>
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5f); // update every half-second to not lag the game

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseCarrierComponent, MapInitEvent>(OnDiseaseCarrierInit);
        SubscribeLocalEvent<DiseaseCarrierComponent, DiseaseCuredEvent>(OnDiseaseCured);

        SubscribeLocalEvent<DiseaseComponent, MapInitEvent>(OnDiseaseInit);
        SubscribeLocalEvent<DiseaseComponent, DiseaseUpdateEvent>(OnUpdateDisease);

        InitializeConditions();
        InitializeEffects();
        InitializeImmunity();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += TimeSpan.FromSeconds(frameTime);
        if (_accumulator < UpdateInterval)
            return;
        _accumulator -= UpdateInterval;

        var diseaseCarriers = EntityQueryEnumerator<DiseaseCarrierComponent>();
        while (diseaseCarriers.MoveNext(out var uid, out var diseaseCarrier))
        {
            UpdateDiseases(uid, diseaseCarrier);
        }
    }

    private void UpdateDiseases(EntityUid uid, DiseaseCarrierComponent diseaseCarrier)
    {
        foreach (var diseaseUid in diseaseCarrier.Diseases)
        {
            var ev = new DiseaseUpdateEvent(uid);
            RaiseLocalEvent(diseaseUid, ev);
        }
    }

    private void OnDiseaseCarrierInit(EntityUid uid, DiseaseCarrierComponent diseaseCarrier, MapInitEvent args)
    {
        foreach (var diseaseId in diseaseCarrier.StartingDiseases)
        {
            TryInfect(uid, diseaseId, out _, diseaseCarrier);
        }
    }

    private void OnDiseaseInit(EntityUid uid, DiseaseComponent disease, MapInitEvent args)
    {
        // check if disease is a preset
        if (disease.StartingEffects.Count == 0)
            return;

        float complexity = 0f;
        foreach (var effectSpecifier in disease.StartingEffects)
        {
            if (TryAdjustEffect(uid, effectSpecifier.Key, out var effect, effectSpecifier.Value, disease))
                complexity += effect.Value.Comp.Severity * effect.Value.Comp.Complexity;
        }
        // disease is a preset so set the complexity
        disease.Complexity = complexity;

        Dirty(uid, disease);
    }

    private void OnDiseaseCured(EntityUid uid, DiseaseCarrierComponent diseaseCarrier, DiseaseCuredEvent args)
    {
        TryCure(uid, args.DiseaseCured.Owner, diseaseCarrier);
    }

    private void OnUpdateDisease(EntityUid uid, DiseaseComponent disease, DiseaseUpdateEvent args)
    {
        var timeDelta = (float)UpdateInterval.TotalSeconds;
        var alive = !_mobState.IsDead(args.Ent) || disease.AffectsDead;

        if (alive)
        {
            foreach (var effectUid in disease.Effects)
            {
                if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                    return;

                var conditionsEv = new DiseaseCheckConditionsEvent(args.Ent, (uid, disease), effect.Severity, disease.InfectionProgress, UpdateInterval);
                RaiseLocalEvent(effectUid, ref conditionsEv);
                if (conditionsEv.DoEffect)
                {
                    var effectEv = new DiseaseEffectEvent(args.Ent, (uid, disease), effect.Severity, disease.InfectionProgress, UpdateInterval);
                    RaiseLocalEvent(effectUid, effectEv);
                }
            }
        }

        var ev = new GetImmunityEvent();
        RaiseLocalEvent(args.Ent, ref ev);

        var immunityStrength = ev.ImmunityStrength * disease.ImmunityProgress;

        // infection progression
        if (alive)
            ChangeInfectionProgress(uid, timeDelta * disease.InfectionRate, disease);
        else
            ChangeInfectionProgress(uid, timeDelta * disease.DeadInfectionRate, disease);

        // immunity
        ChangeInfectionProgress(uid, -timeDelta * ev.ImmunityStrength * disease.ImmunityProgress, disease);
        ChangeImmunityProgress(uid, timeDelta * (ev.ImmunityGainRate * disease.ImmunityGainRate), disease);

        if (disease.InfectionProgress == 0f)
        {
            var curedEv = new DiseaseCuredEvent((uid, disease));
            RaiseLocalEvent(args.Ent, curedEv);
        }
    }

    #region public API

    #region disease

    /// <summary>
    /// Changes infection progress for given disease
    /// </summary>
    public void ChangeInfectionProgress(EntityUid uid, float amount, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.InfectionProgress = Math.Clamp(comp.InfectionProgress + amount, 0f, 1f);
        Dirty(uid, comp);
    }

    /// <summary>
    /// Changes immunity progress for given disease
    /// </summary>
    public void ChangeImmunityProgress(EntityUid uid, float amount, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.ImmunityProgress = Math.Clamp(comp.ImmunityProgress + amount, 0f, 1f);
        Dirty(uid, comp);
    }

    #endregion

    #region disease carriers

    /// <summary>
    /// Finds a disease of specified genotype, if any
    /// </summary>
    public bool FindDisease(EntityUid uid, int genotype, [NotNullWhen(true)] out EntityUid? disease, DiseaseCarrierComponent? comp = null)
    {
        disease = null;
        if (!Resolve(uid, ref comp, false))
            return false;

        foreach (var diseaseUid in comp.Diseases)
        {
            if (!TryComp<DiseaseComponent>(diseaseUid, out var diseaseComp))
                continue;

            if (genotype == diseaseComp.Genotype)
            {
                disease = diseaseUid;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the entity has a disease of specified genotype
    /// </summary>
    public bool HasDisease(EntityUid uid, int genotype, DiseaseCarrierComponent? comp = null)
    {
        return FindDisease(uid, genotype, out _, comp);
    }

    /// <summary>
    /// Tries to cure the entity of the given disease entity
    /// </summary>
    public virtual bool TryCure(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null)
    {
        // does nothing on client
        return false;
    }

    /// <summary>
    /// Tries to infect the entity with the given disease entity
    /// </summary>
    public bool TryInfect(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null, bool force = false)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        if (!TryComp<DiseaseComponent>(disease, out var diseaseComp))
        {
            Log.Error($"Attempted to infect {ToPrettyString(uid)} with disease ToPrettyString{disease}, but it had no DiseaseComponent");
            return false;
        }

        var checkEv = new DiseaseInfectAttemptEvent((disease, diseaseComp));
        if (!force)
            RaiseLocalEvent(uid, ref checkEv);
        // check immunity
        if (!force && (HasDisease(uid, diseaseComp.Genotype, comp) || !checkEv.CanInfect))
            return false;

        comp.Diseases.Add(disease);
        var ev = new DiseaseGainedEvent((disease, diseaseComp));
        RaiseLocalEvent(uid, ev);
        Dirty(uid, comp);
        return true;
    }

    /// <summary>
    /// Tries to infect the entity with a given disease prototype
    /// </summary>
    public virtual bool TryInfect(EntityUid uid, EntProtoId diseaseId, [NotNullWhen(true)] out EntityUid? disease, DiseaseCarrierComponent? comp = null, bool force = false)
    {
        // does nothing on client
        disease = null;
        return false;
    }

    #endregion

    #endregion
}
