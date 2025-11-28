using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.GameTicking;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Goobstation.Shared.Inventory;

namespace Content.Shared._Goobstation.Security.ContrabandIcons;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<VisibleContrabandComponent, GotEquippedEvent>(OnInsertedIntoInventory);
    }

    private void OnInsertedIntoInventory(EntityUid uid, VisibleContrabandComponent component,
        GotEquippedEvent args)
    {
        
    }

    public sealed class ContrabandCheckContainers : InventorySystem
    {
        public bool TryContrabandCheckItems(Entity<InventoryComponent?> entity, SlotFlags desired,
            out Entity<ContrabandComponent?> target)
        {
            if (TryGetContainerSlotEnumerator(entity.Owner, out var containerSlotEnumerator))
            {
                while (containerSlotEnumerator.NextItem(out var item, out var slot))
                {
                    if (!TryComp<ContrabandComponent>(item, out var required))
                        continue;

                    if (((desired & slot.SlotFlags) == 0x0))
                        continue;

                    target = (item, required);
                    return true;
                }
            }

            target = EntityUid.Invalid;
            return false;
        }
    }
}