// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Temperature.Components;

/// <summary>
///     Used to ensure that LowTempImmunityComponent is not overriden (when it is made eventually)
/// </summary>
[RegisterComponent]
public sealed partial class SpecialLowTempImmunityComponent : Component
{
    public override bool SessionSpecific => true;
}
