// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Chemistry;
using Content.Goobstation.Shared.Clothing;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Flashbang;
using Content.Goobstation.Shared.Grab;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Goobstation.Shared.Stunnable;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._White.Standing;
using Content.Shared.Flash;
using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Shared.Inventory;

public partial class GoobInventorySystem
{
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
        SubscribeLocalEvent<InventoryComponent, GetMessageColorOverrideEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, ClothingAutoInjectRelayedEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, ModifyStunTimeEvent>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GrabModifierEvent>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, IsEyesCoveredCheckEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<Overlays.NightVisionComponent>>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<Overlays.ThermalVisionComponent>>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<ShowContrabandIconsComponent>>(RefRelayInventoryEvent);

        // disease
        SubscribeLocalEvent<InventoryComponent, DiseaseOutgoingSpreadAttemptEvent>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, DiseaseIncomingSpreadAttemptEvent>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<ShowDiseaseIconsComponent>>(RefRelayInventoryEvent);

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
