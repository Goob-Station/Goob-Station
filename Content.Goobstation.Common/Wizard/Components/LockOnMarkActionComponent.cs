// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LockOnMarkActionComponent : Component
{
    [DataField]
    public float LockOnRadius = 3f;
}