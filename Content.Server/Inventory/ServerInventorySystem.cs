// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Explosion;
using Content.Shared.Inventory;

namespace Content.Server.Inventory
{
    public sealed class ServerInventorySystem : InventorySystem
    {
        [Dependency] private readonly ToggleableClothingSystem _toggleableClothing = default!; // Goob edit

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InventoryComponent, BeforeExplodeEvent>(OnExploded);
        }

        private void OnExploded(Entity<InventoryComponent> ent, ref BeforeExplodeEvent args)
        {
            // explode each item in their inventory too
            var slots = new InventorySlotEnumerator(ent);
            while (slots.MoveNext(out var slot))
            {
                if (slot.ContainedEntity != null)
                    args.Contents.Add(slot.ContainedEntity.Value);
            }
        }

        public void TransferEntityInventories(Entity<InventoryComponent?> source, Entity<InventoryComponent?> target, bool force = true) // Goob edit
        {
            if (!Resolve(source.Owner, ref source.Comp) || !Resolve(target.Owner, ref target.Comp))
                return;

            // Goob edit start
            _toggleableClothing.SetInventoryTransferring(source, true);
            try
            {
                var enumerator = new InventorySlotEnumerator(source.Comp);
                List<(EntityUid, SlotDefinition)> items = new();
                while (enumerator.NextItem(out var item, out var slot))
                {
                    items.Add((item, slot));
                }
                foreach (var (item, slot) in items)
                {
                    TryUnequip(source, slot.Name, true, force, inventory: source.Comp, triggerHandContact: true);
                    TryEquip(target, item, slot.Name, true, force, inventory: target.Comp, triggerHandContact: true);
                }
            }
            finally
            {
                _toggleableClothing.SetInventoryTransferring(source, false);
            }
            // Goob edit end
        }
    }
}