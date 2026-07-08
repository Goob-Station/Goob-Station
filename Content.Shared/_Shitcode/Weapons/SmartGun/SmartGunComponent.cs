// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmartGunComponent : Component
{
    [DataField]
    public bool RequiresWield;
}