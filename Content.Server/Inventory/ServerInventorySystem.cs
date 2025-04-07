using Content.Shared.Explosion;
using Content.Shared.Inventory;

namespace Content.Server.Inventory
{
    public sealed class ServerInventorySystem : InventorySystem
    {
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

            var enumerator = new InventorySlotEnumerator(source.Comp);
            // Goob edit start
            List<(EntityUid, SlotDefinition)> items = new();
            while (enumerator.NextItem(out var item, out var slot))
            {
                items.Add((item, slot));
            }
            foreach (var (item, slot) in items)
            {
                TryUnequip(source, slot.Name, true, force, inventory: source.Comp);
                TryEquip(target, item, slot.Name , true, force, inventory: target.Comp);
            }
            // Goob edit end
        }
    }
}
