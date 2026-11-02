using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModArmorModuleSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnDamageModify(Entity<ModSuitComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (TryComp<ModArmorModuleComponent>(module, out var armor)
                && TryComp<ModModuleComponent>(module, out var moduleComp)
                && moduleComp.Active)
                args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, armor.Modifiers);
        }
    }
}
