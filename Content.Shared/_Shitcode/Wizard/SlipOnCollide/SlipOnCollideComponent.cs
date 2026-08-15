// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.SlipOnCollide;

[RegisterComponent, NetworkedComponent]
public sealed partial class SlipOnCollideComponent : Component
{
    [DataField]
    public bool Force = true;
}