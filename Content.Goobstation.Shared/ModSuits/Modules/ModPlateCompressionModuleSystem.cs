using Content.Shared.Item;
using Content.Shared.Storage;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModPlateCompressionModuleSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedItemSystem _item = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [SubscribeLocalEvent]
    private void OnInstalled(Entity<ModPlateCompressionModuleComponent> ent, ref ModModuleInstalledEvent args)
    {
        if (HasComp<ItemComponent>(args.Suit))
            _item.SetSize(args.Suit, ent.Comp.Size);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<ModPlateCompressionModuleComponent> ent, ref ModModuleRemovedEvent args)
    {
        if (TerminatingOrDeleted(args.Suit)
            || !HasComp<ItemComponent>(args.Suit)
            || MetaData(args.Suit).EntityPrototype is not { } proto
            || !proto.TryGetComponent<ItemComponent>(out var baseline, Factory))
            return;

        _item.SetSize(args.Suit, baseline.Size);

        if (_container.TryGetContainingContainer(args.Suit, out var container) && HasComp<StorageComponent>(container.Owner))
            _transform.DropNextTo(args.Suit, container.Owner);
    }
}
