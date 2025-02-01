using Content.Shared.Disease;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private TimeSpan _accumulator = TimeSpan.FromSeconds(0);
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(0.5f); // update every half-second to not lag the game

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseCarrierComponent, ComponentStartup>(OnDiseaseCarrierAdded);
        SubscribeLocalEvent<DiseaseCarrierComponent, DiseaseCuredEvent>(OnDiseaseCured);
        SubscribeLocalEvent<DiseaseComponent, ComponentStartup>(OnDiseaseAdded);
        SubscribeLocalEvent<DiseaseComponent, DiseaseUpdateEvent>(OnUpdateDisease);

        InitializeEffects();
        InitializeImmunity();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += TimeSpan.FromSeconds(frameTime);
        if (_accumulator < _updateInterval)
            return;
        _accumulator -= _updateInterval;

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

    private void OnDiseaseCarrierAdded(EntityUid uid, DiseaseCarrierComponent diseaseCarrier, ComponentStartup args)
    {
        foreach (var diseaseId in diseaseCarrier.StartingDiseases)
        {
            TryInfect(uid, diseaseId, out _, diseaseCarrier);
        }
    }

    private void OnDiseaseCured(EntityUid uid, DiseaseCarrierComponent diseaseCarrier, DiseaseCuredEvent args)
    {
        TryCure(uid, args.DiseaseCured.Owner, diseaseCarrier);
    }

    private void OnDiseaseAdded(EntityUid uid, DiseaseComponent disease, ComponentStartup args)
    {
        foreach (var effectId in disease.StartingEffects)
        {
            TryAddEffect(uid, effectId, out _, disease);
        }
    }

    private void OnUpdateDisease(EntityUid uid, DiseaseComponent disease, DiseaseUpdateEvent args)
    {
        var timeDelta = (float)_updateInterval.TotalSeconds;
        var alive = !_mobState.IsDead(args.Ent) || disease.AffectsDead;

        if (alive)
        {
            foreach (var effectUid in disease.Effects)
            {
                if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                    return;

                var effectEv = new DiseaseEffectEvent(args.Ent, (uid, disease), timeDelta * disease.InfectionProgress * effect.Severity);
                RaiseLocalEvent(effectUid, effectEv);
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

        Dirty(uid, disease);
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
    }

    /// <summary>
    /// Changes immunity progress for given disease
    /// </summary>
    public void ChangeImmunityProgress(EntityUid uid, float amount, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.ImmunityProgress = Math.Clamp(comp.ImmunityProgress + amount, 0f, 1f);
    }

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
    public bool TryCure(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null)
    {
        if (_net.IsClient || !Resolve(uid, ref comp))
            return false;

        if (comp.Diseases.Remove(disease))
            QueueDel(disease);
        else
            return false;

        Dirty(uid, comp);
        return true;
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
    public bool TryInfect(EntityUid uid, EntProtoId diseaseId, [NotNullWhen(true)] out EntityUid? disease, DiseaseCarrierComponent? comp = null, bool force = false)
    {
        disease = null;
        if (_net.IsClient || !Resolve(uid, ref comp, false))
            return false;

        var spawned = Spawn(diseaseId);
        if (!TryInfect(uid, spawned, comp, force))
        {
            QueueDel(spawned);
            return false;
        }
        disease = spawned;
        return true;
    }

    #endregion

    #endregion
}
