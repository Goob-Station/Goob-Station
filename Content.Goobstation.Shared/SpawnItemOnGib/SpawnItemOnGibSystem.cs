using Content.Shared.Body.Events;

namespace Content.Goobstation.Shared.SpawnItemOnGib;

public sealed partial class SpawnItemOnGibSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnItemsOnGibComponent, BeingGibbedEvent>(OnGibbed);
    }

    private void OnGibbed(Entity<SpawnItemsOnGibComponent> entity, ref BeingGibbedEvent args)
    {
        foreach (var (item, quantity) in entity.Comp.ItemsToSpawn)
            for (var i = quantity - 1; i >= 0; i--)
                PredictedSpawnAtPosition(item, Transform(entity).Coordinates);

    }
}
