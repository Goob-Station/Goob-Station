// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Mech.Malfunctions;

[RegisterComponent]
public sealed partial class CabinOnFireComponent : Component
{
}

/// <summary>
/// Raises when CabinOnFireEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class CabinOnFireEvent : BaseMalfunctionEvent
{
}
