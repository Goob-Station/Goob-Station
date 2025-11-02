using Content.Shared.Clothing;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Lavaland.Clothing;

/// <summary>
/// This handles cremating of wearer when clothing gets removed
/// </summary>
public sealed class AshOnRemovalSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared._Lavaland.Clothing.AshOnRemovalComponent,ClothingGotUnequippedEvent>(OnUnequip);
    }

    private void OnUnequip(Entity<Shared._Lavaland.Clothing.AshOnRemovalComponent> ent,ref ClothingGotUnequippedEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        var user = args.Wearer;

        _audio.PlayPredicted(ent.Comp.Sound, ent.Owner, user);

        Spawn("Ash", Transform(user).Coordinates);
        Strip(user);
        QueueDel(user);
    }

    private void Strip(EntityUid uid)
    {
        if (TryComp<InventoryComponent>(uid, out var inventory))
        {
            var slots = _inventory.GetSlotEnumerator((uid, inventory));
            while (slots.NextItem(out _, out var slot))
                _inventory.TryUnequip(uid, uid, slot.Name, true, true, inventory: inventory);

        }

        if (TryComp<HandsComponent>(uid, out var hands))
        {
            foreach (var hand in _hands.EnumerateHands((uid, hands)))
            {
                _hands.TryDrop((uid, hands),
                    hand,
                    checkActionBlocker: false,
                    doDropInteraction: false);
            }
        }
    }
}
