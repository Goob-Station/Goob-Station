// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Block;

[RegisterComponent, NetworkedComponent]
public sealed partial class BlockChargeUserComponent : Component
{
    [ViewVariables]
    public EntityUid BlockingWeapon;
}