// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Hood.Weapons;

/// <summary>
/// Gives a firearm one focused switch slot and defines its modes while no switch is attached.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunSwitchSystem))]
public sealed partial class GunSwitchCompatibleComponent : Component
{
    public const string SwitchSlotId = "hood-gun-switch";

    [DataField]
    public ItemSlot SwitchSlot = new();

    /// <summary>
    /// Fire modes restored whenever the switch is absent. Keep this free of FullAuto on normal Glorp models.
    /// </summary>
    [DataField]
    public SelectiveFire BaseModes = SelectiveFire.SemiAuto;
}
