// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Goobstation.Shared.Shadowling;

/// <summary>
/// Added to mind role entities to tag that they are a shadowling.
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingRoleComponent : BaseMindRoleComponent
{
    /// <summary>
    /// Used for round-end text. Indicates how many thralls the Shadowling converted during the round.
    /// </summary>
    [DataField]
    public int ThrallsConverted;
}
