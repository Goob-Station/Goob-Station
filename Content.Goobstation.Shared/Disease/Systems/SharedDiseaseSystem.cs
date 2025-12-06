using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Disease.Systems;

public abstract partial class SharedDiseaseSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private TimeSpan _lastUpdated = TimeSpan.FromSeconds(0);

    protected EntProtoId BaseDisease = "DiseaseBase";

    /// <summary>
    /// The interval between updates of disease and disease effect entities
    /// </summary>
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(0.5f); // update every half-second to not lag the game

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


        if (_timing.CurTime < _lastUpdated + _updateInterval)
            return;

        _lastUpdated += _updateInterval;

        if (!_timing.IsFirstTimePredicted)
            return;

        var diseaseCarriers = EntityQueryEnumerator<DiseaseCarrierComponent>();
        // so that we can EnsureComp disease carriers while we're looping over them without erroring
        List<Entity<DiseaseCarrierComponent>> carriers = new();
        while (diseaseCarriers.MoveNext(out var uid, out var diseaseCarrier))
        {
            carriers.Add((uid, diseaseCarrier));
        }
        for (var i = 0; i < carriers.Count; i++)
        {
            UpdateDiseases(carriers[i].Owner, carriers[i].Comp);
        }
    }

    private void UpdateDiseases(EntityUid uid, DiseaseCarrierComponent diseaseCarrier)
    {
        // not foreach since it can be cured and deleted from the list while inside the loop
        foreach (var diseaseUid in diseaseCarrier.Diseases)
        {
            var ev = new DiseaseUpdateEvent((uid, diseaseCarrier));
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

        var complexity = 0f;
        foreach (var effectSpecifier in disease.StartingEffects)
        {
            if (TryAdjustEffect(uid, effectSpecifier.Key, out var effect, effectSpecifier.Value, disease))
                complexity += effect.Value.Comp.GetComplexity();
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
        var timeDelta = (float)_updateInterval.TotalSeconds;
        var alive = !_mobState.IsDead(args.Ent.Owner) || disease.AffectsDead;

        if (alive && !args.Ent.Comp.EffectImmune)
        {
            foreach (var effectUid in disease.Effects)
            {
                if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                    continue;

                var conditionsEv = new DiseaseCheckConditionsEvent(args.Ent.Owner, (uid, disease), effect);
                RaiseLocalEvent(effectUid, ref conditionsEv);
                if (!conditionsEv.DoEffect)
                    continue;
                var effectEv = new DiseaseEffectEvent(args.Ent.Owner, (uid, disease), effect);
                RaiseLocalEvent(effectUid, effectEv);
            }
        }

        var ev = new GetImmunityEvent((uid, disease));
        // don't even check immunity if we can't affect this disease
        if (CanImmunityAffect(args.Ent.Owner, disease))
            RaiseLocalEvent(args.Ent.Owner, ref ev);

        // infection progression
        if (alive)
            ChangeInfectionProgress(uid, timeDelta * disease.InfectionRate, disease);
        else
            ChangeInfectionProgress(uid, timeDelta * disease.DeadInfectionRate, disease);

        // immunity
        ChangeInfectionProgress(uid, -timeDelta * ev.ImmunityStrength * disease.ImmunityProgress, disease);
        ChangeImmunityProgress(uid, timeDelta * (ev.ImmunityGainRate * disease.ImmunityGainRate), disease);

        if (!(disease.InfectionProgress <= 0f))
            return;
        var curedEv = new DiseaseCuredEvent((uid, disease));
        RaiseLocalEvent(args.Ent.Owner, curedEv);
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

        comp.InfectionProgress = Math.Min(comp.InfectionProgress + amount, 1f);
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

    public bool HasAnyDisease(EntityUid uid, DiseaseCarrierComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        return comp.Diseases.Count != 0;
    }

    /// <summary>
    /// Finds a disease of specified genotype, if any
    /// </summary>
    private bool FindDisease(EntityUid uid, int genotype, [NotNullWhen(true)] out EntityUid? disease, DiseaseCarrierComponent? comp = null)
    {
        disease = null;
        if (!Resolve(uid, ref comp, false))
            return false;

        foreach (var diseaseUid in comp.Diseases)
        {
            if (!TryComp<DiseaseComponent>(diseaseUid, out var diseaseComp))
                continue;

            if (genotype != diseaseComp.Genotype)
                continue;
            disease = diseaseUid;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the entity has a disease of specified genotype
    /// </summary>
    private bool HasDisease(EntityUid uid, int genotype, DiseaseCarrierComponent? comp = null)
    {
        return FindDisease(uid, genotype, out _, comp);
    }

    /// <summary>
    /// Tries to cure the entity of the given disease entity
    /// </summary>
    protected virtual bool TryCure(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null)
    {
        // does nothing on client
        return false;
    }

    /// <summary>
    /// Tries to infect the entity with the given disease entity
    /// Does not clone the provided disease entity, use <see cref="TryClone"/> for that
    /// </summary>
    public bool TryInfect(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null, bool force = false)
    {
        if (force)
            EnsureComp<DiseaseCarrierComponent>(uid, out comp);

        if (!Resolve(uid, ref comp, false))
            return false;

        if (!TryComp<DiseaseComponent>(disease, out var diseaseComp))
        {
            Log.Error($"Attempted to infect {ToPrettyString(uid)} with disease ToPrettyString{disease}, but it had no DiseaseComponent");
            return false;
        }

        var checkEv = new DiseaseInfectAttemptEvent((disease, diseaseComp));
        RaiseLocalEvent(uid, ref checkEv);
        // check immunity
        if (!force && (HasDisease(uid, diseaseComp.Genotype, comp) || !checkEv.CanInfect))
            return false;

        _transform.SetCoordinates(disease, new EntityCoordinates(uid, Vector2.Zero));
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
