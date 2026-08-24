// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Body.Components;

/// <summary>
///     Used to ensure that BreathingImmunityComponent is not overriden.
/// </summary>
[RegisterComponent]
public sealed partial class SpecialBreathingImmunityComponent : Component
{
    public override bool SessionSpecific => true;
}
