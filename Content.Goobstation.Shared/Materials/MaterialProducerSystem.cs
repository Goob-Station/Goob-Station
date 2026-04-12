using Content.Shared.Materials;
using Robust.Shared.Timing;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Power.Components;

namespace Content.Goobstation.Shared.Materials;

public class MaterialProducer : EntitySystem
{
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MaterialProducerComponent, MaterialStorageComponent>();
        while (query.MoveNext(out var uid, out var producer, out var storage))
        {
            if (_timing.CurTime < producer.NextProduceTime)
                continue;

            producer.NextProduceTime = _timing.CurTime + producer.ProductionDelay;

            if (!producer.IsWorkingWithoutElectricity
                && !_power.IsPowered(uid))
                continue;

            _materialStorage.TryChangeMaterialAmount((uid, storage), producer.MaterialsProduce);
        }
    }
}
