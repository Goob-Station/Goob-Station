// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Mech.Malfunctions;

[RegisterComponent]
public sealed partial class EngineBrokenComponent : Component
{
}

/// <summary>
/// Raises when EngineBrokenEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class EngineBrokenEvent : BaseMalfunctionEvent
{
}
