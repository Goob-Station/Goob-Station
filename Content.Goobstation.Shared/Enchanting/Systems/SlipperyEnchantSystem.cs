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
        comp.ParalyzeTime *= factor;
        comp.LaunchForwardsMultiplier *= factor;
        comp.SuperSlippery = true; // needed to actually launch people
        Dirty(item, comp);
    }
}
