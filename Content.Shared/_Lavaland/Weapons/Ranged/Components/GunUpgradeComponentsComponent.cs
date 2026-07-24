// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Weapons.Ranged.Components;

/// <summary>
/// Adds components when inserted and removes them when ejected from a weapon.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunUpgradeComponentsComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
