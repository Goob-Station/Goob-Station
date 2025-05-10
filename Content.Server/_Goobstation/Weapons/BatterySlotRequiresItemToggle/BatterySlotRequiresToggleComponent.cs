// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Weapons.BatterySlotRequiresItemToggle;

[RegisterComponent]
public sealed partial class BatterySlotRequiresToggleComponent : Component
{
    [DataField(required: true)]
    public string ItemSlot = string.Empty;

    [DataField]
    public bool Inverted;
}
