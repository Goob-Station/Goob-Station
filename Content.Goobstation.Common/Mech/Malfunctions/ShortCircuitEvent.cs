// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Mech.Malfunctions;

/// <summary>
/// Raises when ShortCircuitEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class ShortCircuitEvent : BaseMalfunctionEvent
{
    [DataField]
    public float EnergyLoss = 30;

    public ShortCircuitEvent(float energyLoss)
    {
        EnergyLoss = energyLoss;
    }
}
