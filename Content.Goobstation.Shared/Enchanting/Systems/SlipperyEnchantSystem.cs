using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Slippery;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Controls <see cref="SlipperyComponent"/> values with the enchant level.
/// </summary>
public sealed class SlipperyEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlipperyEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<SlipperyEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
    }

    private void OnAdded(Entity<SlipperyEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        Modify(args.Item, ent.Comp.BaseModifier * (float) args.Level);
    }

    private void OnUpgraded(Entity<SlipperyEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        Modify(args.Item, (float) args.Level / (float) args.OldLevel);
    }

    private void Modify(EntityUid item, float factor)
    {
        var comp = EnsureComp<SlipperyComponent>(item);
        var sliptime = 1.5f; // hardcode sliptime here probably reaadd sliptime at some point or smth
        sliptime *= factor; // shitcoding it this way because now stuntime needs a timespan and i dont trust it multiplying a float.
        comp.SlipData.StunTime = TimeSpan.FromSeconds(sliptime);
        comp.SlipData.LaunchForwardsMultiplier *= factor;
        comp.SlipData.SuperSlippery = true; // needed to actually launch people
        Dirty(item, comp);
    }
}
