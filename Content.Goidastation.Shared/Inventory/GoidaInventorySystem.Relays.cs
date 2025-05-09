// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Shared.Chemistry;
using Content.Goidastation.Shared.Clothing;
using Content.Goidastation.Shared.Flashbang;
using Content.Goidastation.Shared.Stunnable;
using Content.Shared._Goidastation.Wizard.Chuuni;
using Content.Shared._White.Standing;
using Content.Shared.Damage.Events;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;

namespace Content.Goidastation.Shared.Inventory;

public partial class GoidaInventorySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public void InitializeRelays()
    {
        base.Initialize();
        SubscribeLocalEvent<InventoryComponent, DelayedKnockdownAttemptEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, VaporCheckEyeProtectionEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, CheckMagicItemEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetFlashbangedEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, FlashDurationMultiplierEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetStandingUpTimeMultiplierEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetSpellInvocationEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetMessagePostfixEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, ClothingAutoInjectRelayedEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, ModifyStunTimeEvent>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<Overlays.NightVisionComponent>>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, TakeStaminaDamageEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<Overlays.ThermalVisionComponent>>(RefRelayInventoryEvent);
    }

    private void RefRelayInventoryEvent<T>(EntityUid uid, InventoryComponent component, ref T args) where T : IInventoryRelayEvent
    {
        _inventorySystem.RelayEvent((uid, component), ref args);
    }

    private void RelayInventoryEvent<T>(EntityUid uid, InventoryComponent component, T args) where T : IInventoryRelayEvent
    {
        _inventorySystem.RelayEvent((uid, component), args);
    }
}
