using Content.Shared.Storage;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModStorageModuleSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModStorageModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (HasComp<StorageComponent>(args.Suit))
            return;

        EntityManager.AddComponents(args.Suit, ent.Comp.Components);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModStorageModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (!TryComp<StorageComponent>(args.Suit, out var storage))
            return;

        var coords = _transform.GetMoverCoordinates(args.Suit);
        _container.EmptyContainer(storage.Container, true, coords);
        RemComp(args.Suit, storage);
    }
}
