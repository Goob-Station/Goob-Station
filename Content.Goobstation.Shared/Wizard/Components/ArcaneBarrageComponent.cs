// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ArcaneBarrageComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Unremoveable = true;
}