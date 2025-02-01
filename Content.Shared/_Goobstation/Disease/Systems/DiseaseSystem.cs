using Content.Shared.Disease;
using Robust.Shared.GameObjects;
using System;

namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem : EntitySystem
{
    private TimeSpan _accumulator = TimeSpan.FromSeconds(0);
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(0.5f); // update every half-second to not lag the game

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseComponent, DiseaseUpdateEvent>(OnUpdateDisease);
        SubscribeLocalEvent<DiseaseCarrierComponent, DiseaseCuredEvent>(OnDiseaseCured);
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

    private void OnUpdateDisease(EntityUid uid, DiseaseComponent disease, DiseaseUpdateEvent args)
    {
        var timeDelta = (float)_updateInterval.TotalSeconds;

        foreach (var effectUid in disease.Effects)
        {
            if (!TryComp<DiseaseEffectComponent>(effectUid, out var effect))
                return;

            var effectEv = new DiseaseEffectEvent(args.Ent, timeDelta * disease.InfectionProgress * effect.Severity);
            RaiseLocalEvent(effectUid, effectEv);
        }

        var ev = new GetImmunityEvent();
        RaiseLocalEvent(args.Ent, ref ev);

        disease.InfectionProgress += disease.InfectionRate - ev.ImmunityStrength * disease.ImmunityProgress;
        disease.InfectionProgress = Math.Clamp(disease.InfectionProgress, 0f, 1f);

        disease.ImmunityProgress += ev.ImmunityGainRate;
        disease.ImmunityProgress = Math.Clamp(disease.ImmunityProgress, 0f, 1f);

        if (disease.InfectionProgress == 0f)
        {
            var curedEv = new DiseaseCuredEvent(uid);
            RaiseLocalEvent(args.Ent, curedEv);
        }
    }

    private void OnDiseaseCured(EntityUid uid, DiseaseCarrierComponent diseaseCarrier, DiseaseCuredEvent args)
    {
        diseaseCarrier.Diseases.Remove(args.DiseaseCured);
        QueueDel(args.DiseaseCured);
    }
}
