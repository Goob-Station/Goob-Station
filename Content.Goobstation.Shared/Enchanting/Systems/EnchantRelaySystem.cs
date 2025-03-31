using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Relays events from enchanted items to their enchants.
/// </summary>
public sealed class EnchantRelaySystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnchantedComponent, DamageModifyEvent>(RelayEvent);
        SubscribeLocalEvent<EnchantedComponent, MeleeHitEvent>(RelayEvent);
        SubscribeLocalEvent<EnchantedComponent, AttackedEvent>(RelayEvent); // so thorns applies if you punch the armor
        SubscribeLocalEvent<EnchantedComponent, InventoryRelayedEvent<AttackedEvent>>(RelayInventoryEvent);
        SubscribeLocalEvent<EnchantedComponent, StepTriggerAttemptEvent>(RelayEvent); // lava mouse
        SubscribeLocalEvent<EnchantedComponent, InventoryRelayedEvent<StepTriggerAttemptEvent>>(RelayInventoryEvent);

        SubscribeLocalEvent<InventoryComponent, AttackedEvent>(_inventory.RelayEvent);
        SubscribeLocalEvent<InventoryComponent, StepTriggerAttemptEvent>(_inventory.RelayEvent);
    }

    private void RelayEvent<T>(Entity<EnchantedComponent> ent, ref T args)
    {
        foreach (var enchant in ent.Comp.Enchants)
        {
            RaiseLocalEvent(enchant, args);
        }
    }

    private void RelayInventoryEvent<T>(Entity<EnchantedComponent> ent, ref InventoryRelayedEvent<T> args) where T: IInventoryRelayEvent
    {
        RelayEvent(ent, ref args.Args);
    }
}
