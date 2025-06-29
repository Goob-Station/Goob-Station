// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Inventory;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// This is used for relaying ammo events
/// to an entity in the user's clothing slot.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunSystem))]
public sealed partial class ClothingSlotAmmoProviderComponent : AmmoProviderComponent
{
    /// <summary>
    /// The slot that the ammo provider should be located in.
    /// </summary>
    [DataField("targetSlot", required: true)]
    public SlotFlags TargetSlot;

    /// <summary>
    /// A whitelist for determining whether or not an ammo provider is valid.
    /// </summary>
    [DataField("providerWhitelist")]
    public EntityWhitelist? ProviderWhitelist;

    /// <summary>
    /// Assmos - Extinguisher Nozzle
    /// If the hands are considered a valid ammo provider slot.
    /// </summary>
    [DataField("checkHands")]
    public bool CheckHands = false;
}
