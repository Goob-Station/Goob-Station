using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Systems;
using Content.Server.Objectives.Systems;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles the Shadowling's System
/// </summary>
public sealed class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingAscendEvent>(OnAscend);
    }

    private void OnAscend(ShadowlingAscendEvent args)
    {
        if (TryComp<ShadowlingComponent>(args.ShadowlingAscended, out var comp))
            _codeCondition.SetCompleted(args.ShadowlingAscended, comp.ObjectiveAscend);
    }

    protected override void StartHatchingProgress(Entity<ShadowlingComponent> ent)
    {
        var (uid, comp) = ent;

        comp.IsHatching = true;

        // Drop all items
        if (TryComp<InventoryComponent>(uid, out var inv))
        {
            foreach (var slot in inv.Slots)
            {
                _inventorySystem.DropSlotContents(uid, slot.Name, inv);
            }
        }

        var egg = SpawnAtPosition(comp.Egg, Transform(uid).Coordinates);
        if (TryComp<HatchingEggComponent>(egg, out var eggComp)
            && TryComp<EntityStorageComponent>(egg, out var eggStorage))
        {
            eggComp.ShadowlingInside = uid;
            _entityStorage.Insert(uid, egg, eggStorage);
        }

        // It should be noted that Shadowling shouldn't be able to take damage during this process.
    }
}
