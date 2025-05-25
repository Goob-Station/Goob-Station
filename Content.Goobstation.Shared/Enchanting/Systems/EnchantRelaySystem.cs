// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Electrocution;
using Content.Shared.Inventory;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Temperature;
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

        SubInventory<DamageModifyEvent>();
        SubscribeLocalEvent<EnchantedComponent, MeleeHitEvent>(RelayEvent);
        SubInventory<AttackedEvent>(true);
        SubInventory<StepTriggerAttemptEvent>(true);
        SubInventory<GetFireProtectionEvent>();
        SubInventory<ModifyChangedTemperatureEvent>();
        SubInventory<ElectrocutionAttemptEvent>();
    }

    private void SubInventory<T>(bool relayInventory = false) where T: IInventoryRelayEvent
    {
        SubscribeLocalEvent<EnchantedComponent, T>(RelayEvent);
        SubscribeLocalEvent<EnchantedComponent, InventoryRelayedEvent<T>>(RelayInventoryEvent);
        // only needed if the source system doesn't relay directly and inventory system doesn't relay for it
        if (relayInventory)
            SubscribeLocalEvent<InventoryComponent, T>(_inventory.RelayEvent);
    }

    private void RelayEvent<T>(Entity<EnchantedComponent> ent, ref T args) where T : notnull
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
