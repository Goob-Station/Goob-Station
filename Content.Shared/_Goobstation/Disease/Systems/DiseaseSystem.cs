using Content.Shared.Disease;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System;

namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem : EntitySystem
{
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
        SubscribeLocalEvent<ImmunityComponent, GetImmunityEvent>(OnGetImmunity);

        InitializeEffects();
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
            TryInfect(uid, diseaseId, diseaseCarrier);
        }
    }

    private void OnDiseaseCured(EntityUid uid, DiseaseCarrierComponent diseaseCarrier, DiseaseCuredEvent args)
    {
        diseaseCarrier.Diseases.Remove(args.DiseaseCured);
        QueueDel(args.DiseaseCured);
    }

    private void OnDiseaseAdded(EntityUid uid, DiseaseComponent disease, ComponentStartup args)
    {
        foreach (var effectId in disease.StartingEffects)
        {
            TryAddEffect(uid, effectId, disease);
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
            var curedEv = new DiseaseCuredEvent(uid);
            RaiseLocalEvent(args.Ent, curedEv);
        }
    }

    private void OnGetImmunity(EntityUid uid, ImmunityComponent immunity, ref GetImmunityEvent args)
    {
        if (!_mobState.IsDead(uid) || immunity.InDead)
        {
            args.ImmunityGainRate += immunity.ImmunityGainRate;
            args.ImmunityStrength += immunity.ImmunityStrength;
        }
    }

    public void ChangeInfectionProgress(EntityUid uid, float amount, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.InfectionProgress = Math.Clamp(comp.InfectionProgress + amount, 0f, 1f);
    }

    public void ChangeImmunityProgress(EntityUid uid, float amount, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.ImmunityProgress = Math.Clamp(comp.ImmunityProgress + amount, 0f, 1f);
    }

    public bool TryInfect(EntityUid uid, EntProtoId diseaseId, DiseaseCarrierComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        var disease = Spawn(diseaseId);
        comp.Diseases.Add(disease);
        return true;
    }

    public bool TryAddEffect(EntityUid uid, EntProtoId effectId, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        var effect = Spawn(effectId);
        comp.Effects.Add(effect);
        return true;
    }
}
