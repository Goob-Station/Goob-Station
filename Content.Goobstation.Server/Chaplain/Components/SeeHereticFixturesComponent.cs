// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Chaplain.Components;

/// <summary>
/// Gives the user the ability to see the Eldritch Influence layer.
/// </summary>
[RegisterComponent]
public sealed partial class SeeHereticFixturesComponent : Component
{
    [DataField]
    public bool SeeShifts = true;

    [DataField]
    public bool SeeFractures = true;
}
