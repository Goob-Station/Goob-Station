using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Clothing.Systems;

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
        SubscribeLocalEvent<AshOnRemovalComponent,ClothingGotUnequippedEvent>(OnUnequipt);
    }

    private void OnUnequipt(Entity<AshOnRemovalComponent> ent,ref ClothingGotUnequippedEvent arg)
    {

        if (!ent.Comp.Enabled)
            return;
        var user = arg.Wearer;

        _audio.PlayPredicted(ent.Comp.Sound, ent.Owner, user);

        Spawn("Ash", Transform(user).Coordinates);
        Strip(user);
        Del(user);

    }

    private void Strip(EntityUid uid)
    {
        if (TryComp<InventoryComponent>(uid, out var inventory))
        {
            var slots = _inventory.GetSlotEnumerator((uid, inventory));
            while (slots.NextItem(out _, out var slot))
            {
                _inventory.TryUnequip(uid, uid, slot.Name, true, true, inventory: inventory);
            }
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
