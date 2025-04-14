// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Mech.Malfunctions;

/// <summary>
/// Raises when EquipmentLossEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class EquipmentLossEvent : BaseMalfunctionEvent
{
    [DataField]
    public float Range = 3f;
    public EquipmentLossEvent(float range)
    {
        Range = range;
    }
}
