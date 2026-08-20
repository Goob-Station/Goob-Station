using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsDropAllSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    public string Id => "drop-all";
    public string DisplayName => "Drop Everything";
    public string DisplayDescription => "Drop all equipped and held items onto the ground.";
    public CVarDef<string> Sku => GoobCVars.TwitchBitsDropAllSku;

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (!HasAnyItems(target))
            return TwitchBitsActionValidity.Invalid("The streamer has no equipped or held items to drop.");

        return TwitchBitsActionValidity.Valid;
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        var dropped = false;
        if (TryComp<InventoryComponent>(target, out var inventory))
        {
            var slots = _inventory.GetSlotEnumerator((target, inventory));
            while (slots.NextItem(out _, out var slot))
            {
                dropped |= _inventory.TryUnequip(
                    target,
                    target,
                    slot.Name,
                    silent: true,
                    force: true,
                    inventory: inventory);
            }
        }

        if (TryComp<HandsComponent>(target, out var hands))
        {
            foreach (var hand in _hands.EnumerateHands((target, hands)))
            {
                dropped |= _hands.TryDrop(
                    (target, hands),
                    hand,
                    checkActionBlocker: false,
                    doDropInteraction: false);
            }
        }

        return dropped;
    }

    private bool HasAnyItems(EntityUid target)
    {
        if (TryComp<InventoryComponent>(target, out var inventory))
        {
            var slots = _inventory.GetSlotEnumerator((target, inventory));
            if (slots.NextItem(out _, out _))
                return true;
        }

        if (!TryComp<HandsComponent>(target, out var hands))
            return false;

        return _hands.EnumerateHands((target, hands))
            .Any(hand => _hands.GetHeldItem((target, hands), hand) != null);
    }
}
