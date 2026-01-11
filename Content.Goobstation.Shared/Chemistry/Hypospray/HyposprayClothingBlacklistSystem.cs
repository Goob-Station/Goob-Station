using Content.Shared.Chemistry.Hypospray.Events;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Chemistry.Hypospray;

public sealed partial class HyposprayClothingBlacklistSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HyposprayClothingBlacklistComponent, BeforeHyposprayInjectsEvent>
            (BeforeHyposprayInject, after: [typeof(HyposprayWhitelistSystem)]);
    }

    public bool IsValid(EntityUid target, HyposprayClothingBlacklistComponent comp)
    {
        if (!TryComp<InventoryComponent>(target, out var inventory))
            return true;

        var enumerator = _inventory.GetSlotEnumerator((target, inventory), comp.Slots);
        while (enumerator.MoveNext(out var slot))
        {
            if (slot.ContainedEntity.HasValue
                && _entityWhitelist.IsValid(comp.Blacklist, slot.ContainedEntity.Value))
                return false;
        }

        return true;
    }

    private void BeforeHyposprayInject(Entity<HyposprayClothingBlacklistComponent> ent,
        ref BeforeHyposprayInjectsEvent args)
    {
        if (args.Cancelled)
            return;

        if (!IsValid(args.TargetGettingInjected, ent.Comp))
        {
            args.InjectMessageOverride = "hypospray-blacklist-cannot-pierce";
            args.Cancel();
        }
    }
}
