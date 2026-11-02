using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Armor;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModArmorModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedArmorSystem _armor = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModArmorModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        RefreshPartArmor(args.Suit, ent.Comp.Slot);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModArmorModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        RefreshPartArmor(args.Suit, ent.Comp.Slot);
    }

    [SubscribeLocalEvent]
    private void OnDamageModify(Entity<ModSuitComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (TryComp<ModArmorModuleComponent>(module, out var armor)
                && armor.Slot == null
                && TryComp<ModModuleComponent>(module, out var moduleComp)
                && moduleComp.Active)
                args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, armor.Modifiers);
        }
    }

    private void RefreshPartArmor(EntityUid suitUid, string? slot)
    {
        if (slot == null
            || !TryComp<ModSuitComponent>(suitUid, out var suit)
            || _modSuit.GetPart((suitUid, suit), slot) is not { } part
            || TerminatingOrDeleted(part)
            || !TryComp<ArmorComponent>(part, out var armor)
            || MetaData(part).EntityPrototype is not { } proto
            || !proto.TryGetComponent<ArmorComponent>(out var baseline, Factory))
            return;

        var modifiers = new DamageModifierSet
        {
            Coefficients = new(baseline.Modifiers.Coefficients),
            FlatReduction = new(baseline.Modifiers.FlatReduction),
            IgnoreArmorPierceFlags = baseline.Modifiers.IgnoreArmorPierceFlags,
        };

        foreach (var module in suit.ModulesContainer.ContainedEntities)
        {
            if (!TryComp<ModArmorModuleComponent>(module, out var moduleArmor)
                || moduleArmor.Slot != slot
                || !TryComp<ModModuleComponent>(module, out var moduleComp)
                || !moduleComp.Active)
                continue;

            foreach (var (type, coefficient) in moduleArmor.Modifiers.Coefficients)
            {
                modifiers.Coefficients[type] = modifiers.Coefficients.TryGetValue(type, out var existing) ? existing * coefficient : coefficient;
            }

            foreach (var (type, reduction) in moduleArmor.Modifiers.FlatReduction)
            {
                modifiers.FlatReduction[type] = modifiers.FlatReduction.TryGetValue(type, out var existing) ? existing + reduction : reduction;
            }
        }

        _armor.SetModifiers((part, armor), modifiers);
    }
}
