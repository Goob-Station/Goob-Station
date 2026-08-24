// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Weapons.NoWieldNeeded;

/// <summary>
/// Indicates that this gun user does not need to wield.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NoWieldNeededComponent: Component
{
    //If true, not only does the user not need to wield to fire, they get the bonus for free!
    [DataField]
    public bool GetBonus = true;

    public List<EntityUid> GunsWithBonus = [];
}
