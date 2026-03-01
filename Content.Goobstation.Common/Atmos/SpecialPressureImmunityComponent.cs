// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Atmos;

/// <summary>
///     Used to ensure that PressureImmunityComponent is not overriden.
/// </summary>
[RegisterComponent]
public sealed partial class SpecialPressureImmunityComponent : Component
{
    public override bool SessionSpecific => true;
}
