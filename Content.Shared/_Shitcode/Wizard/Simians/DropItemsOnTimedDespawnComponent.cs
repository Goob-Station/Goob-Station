// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Simians;

[RegisterComponent, NetworkedComponent]
public sealed partial class DropItemsOnTimedDespawnComponent : Component
{
    [DataField]
    public bool DropDespawningItems;
}