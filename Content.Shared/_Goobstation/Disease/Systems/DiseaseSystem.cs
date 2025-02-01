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
}
