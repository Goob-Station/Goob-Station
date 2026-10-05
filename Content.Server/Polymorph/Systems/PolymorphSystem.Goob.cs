using System.Linq;
using Content.Shared.Polymorph;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;

namespace Content.Server.Polymorph.Systems;

public sealed partial class PolymorphSystem
{
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    private void TransferStorage(EntityUid old, EntityUid @new, PolymorphInventoryChange inventory)
    {
        if (inventory == PolymorphInventoryChange.None
            || !TryComp<StorageComponent>(old, out var storage))
            return;

        TryComp<StorageComponent>(@new, out var newStorage);
        foreach (var item in storage.Container.ContainedEntities.ToArray())
        {
            if (!_container.Remove(item, storage.Container))
                continue;

            if (inventory == PolymorphInventoryChange.Transfer && newStorage != null)
                _storage.Insert(@new, item, out _, storageComp: newStorage, playSound: false);
        }
    }
}
