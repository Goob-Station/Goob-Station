// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgerySpeedModifierComponent : Component
{
    [DataField]
    public float SpeedModifier = 1.5f;
}