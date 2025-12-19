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

    protected EntityQuery<DiseaseEffectComponent> _effectQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseCarrierComponent, MapInitEvent>(OnDiseaseCarrierInit);
        SubscribeLocalEvent<DiseaseCarrierComponent, DiseaseCuredEvent>(OnDiseaseCured);

        SubscribeLocalEvent<DiseaseComponent, MapInitEvent>(OnDiseaseInit);
        SubscribeLocalEvent<DiseaseComponent, DiseaseUpdateEvent>(OnUpdateDisease);

        _effectQuery = GetEntityQuery<DiseaseEffectComponent>();

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
            UpdateDiseases(carriers[i]);
        }
    }

    private void UpdateDiseases(Entity<DiseaseCarrierComponent> ent)
    {
        // not foreach since it can be cured and deleted from the list while inside the loop
        var diseases = new List<EntityUid>(ent.Comp.Diseases);
        foreach (var diseaseUid in diseases)
        {
            var ev = new DiseaseUpdateEvent(ent);
            RaiseLocalEvent(diseaseUid, ref ev);
        }
    }

    private void OnDiseaseCarrierInit(Entity<DiseaseCarrierComponent> ent, ref MapInitEvent args)
    {
        foreach (var diseaseId in ent.Comp.StartingDiseases)
        {
            TryInfect((ent, ent.Comp), diseaseId, out _);
        }
    }

    private void OnDiseaseInit(Entity<DiseaseComponent> ent, ref MapInitEvent args)
    {
        // check if disease is a preset
        if (ent.Comp.StartingEffects.Count == 0)
            return;

        var complexity = 0f;
        foreach (var effectSpecifier in ent.Comp.StartingEffects)
        {
            if (TryAdjustEffect((ent, ent.Comp), effectSpecifier.Key, out var effect, effectSpecifier.Value))
                complexity += effect.Value.Comp.GetComplexity();
        }
        // disease is a preset so set the complexity
        ent.Comp.Complexity = complexity;

        Dirty(ent);
    }

    private void OnDiseaseCured(Entity<DiseaseCarrierComponent> ent, ref DiseaseCuredEvent args)
    {
        TryCure((ent, ent.Comp), args.Disease);
    }

    private void OnUpdateDisease(Entity<DiseaseComponent> ent, ref DiseaseUpdateEvent args)
    {
        var timeDelta = (float)_updateInterval.TotalSeconds;
        var alive = !_mobState.IsDead(args.Ent.Owner) || ent.Comp.AffectsDead;

        if (!args.Ent.Comp.EffectImmune)
        {
            foreach (var effectUid in ent.Comp.Effects)
            {
                if (!_effectQuery.TryComp(effectUid, out var effect))
                    continue;

                if (!alive)
                {
                    var failEv = new DiseaseEffectFailedEvent(effect, ent, args.Ent);
                    RaiseLocalEvent(effectUid, ref failEv);
                    continue;
                }

                var conditionsEv = new DiseaseCheckConditionsEvent(effect, ent, args.Ent);
                RaiseLocalEvent(effectUid, ref conditionsEv);

                if (!conditionsEv.DoEffect)
                {
                    var failEv = new DiseaseEffectFailedEvent(effect, ent, args.Ent);
                    RaiseLocalEvent(effectUid, ref failEv);
                    continue;
                }

                var effectEv = new DiseaseEffectEvent(effect, ent, args.Ent);
                RaiseLocalEvent(effectUid, ref effectEv);
            }
        }

        var ev = new GetImmunityEvent(ent);
        // don't even check immunity if we can't affect this disease
        if (CanImmunityAffect(args.Ent.Owner, ent.Comp))
            RaiseLocalEvent(args.Ent.Owner, ref ev);

        // infection progression
        if (alive)
            ChangeInfectionProgress((ent, ent.Comp), timeDelta * ent.Comp.InfectionRate);
        else
            ChangeInfectionProgress((ent, ent.Comp), timeDelta * ent.Comp.DeadInfectionRate);

        // immunity
        ChangeInfectionProgress((ent, ent.Comp), -timeDelta * ev.ImmunityStrength * ent.Comp.ImmunityProgress);
        ChangeImmunityProgress((ent, ent.Comp), timeDelta * ev.ImmunityGainRate * ent.Comp.ImmunityGainRate);

        if (ent.Comp.InfectionProgress > 0f)
            return;
        var curedEv = new DiseaseCuredEvent(ent);
        RaiseLocalEvent(args.Ent.Owner, ref curedEv);
    }

    #region public API

    #region disease

    /// <summary>
    /// Changes infection progress for given disease
    /// </summary>
    public void ChangeInfectionProgress(Entity<DiseaseComponent?> ent, float amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.InfectionProgress = Math.Min(ent.Comp.InfectionProgress + amount, 1f);
        Dirty(ent);
    }

    /// <summary>
    /// Changes immunity progress for given disease
    /// </summary>
    public void ChangeImmunityProgress(Entity<DiseaseComponent?> ent, float amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.ImmunityProgress = Math.Clamp(ent.Comp.ImmunityProgress + amount, 0f, 1f);
        Dirty(ent);
    }

    #endregion

    #region disease carriers

    public bool HasAnyDisease(Entity<DiseaseCarrierComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        return ent.Comp.Diseases.Count != 0;
    }

    /// <summary>
    /// Finds a disease of specified genotype, if any
    /// </summary>
    private bool FindDisease(Entity<DiseaseCarrierComponent?> ent, int genotype, [NotNullWhen(true)] out EntityUid? disease)
    {
        disease = null;
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        foreach (var diseaseUid in ent.Comp.Diseases)
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
    private bool HasDisease(Entity<DiseaseCarrierComponent?> ent, int genotype)
    {
        return FindDisease(ent, genotype, out _);
    }

    /// <summary>
    /// Tries to cure the entity of the given disease entity
    /// </summary>
    public virtual bool TryCure(Entity<DiseaseCarrierComponent?> ent, EntityUid disease)
    {
        // does nothing on client
        return false;
    }

    /// <summary>
    /// Tries to cure the entity of all diseases
    /// </summary>
    public virtual bool TryCureAll(Entity<DiseaseCarrierComponent?> ent)
    {
        // does nothing on client
        return false;
    }

    /// <summary>
    /// Tries to infect the entity with the given disease entity
    /// Does not clone the provided disease entity, use <see cref="TryClone"/> for that
    /// </summary>
    public bool TryInfect(Entity<DiseaseCarrierComponent?> ent, EntityUid disease, bool force = false)
    {
        if (force)
            EnsureComp<DiseaseCarrierComponent>(ent, out ent.Comp);

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!TryComp<DiseaseComponent>(disease, out var diseaseComp))
        {
            Log.Error($"Attempted to infect {ToPrettyString(ent)} with disease ToPrettyString{disease}, but it had no DiseaseComponent");
            return false;
        }

        var checkEv = new DiseaseInfectAttemptEvent((disease, diseaseComp));
        RaiseLocalEvent(ent, ref checkEv);
        // check immunity
        if (!force && (HasDisease(ent, diseaseComp.Genotype) || !checkEv.CanInfect))
            return false;

        _transform.SetCoordinates(disease, new EntityCoordinates(ent, Vector2.Zero));
        ent.Comp.Diseases.Add(disease);
        var ev = new DiseaseGainedEvent((disease, diseaseComp));
        RaiseLocalEvent(ent, ref ev);
        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Tries to infect the entity with a given disease prototype
    /// </summary>
    public virtual bool TryInfect(Entity<DiseaseCarrierComponent?> ent, EntProtoId diseaseId, [NotNullWhen(true)] out EntityUid? disease, bool force = false)
    {
        // does nothing on client
        disease = null;
        return false;
    }

    #endregion

    #endregion
}
