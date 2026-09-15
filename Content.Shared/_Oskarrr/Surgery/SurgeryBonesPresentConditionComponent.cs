// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.Surgery;

/// <summary>
/// Surgery is valid only if the targeted part has anatomical bones (or lacks them when inverted).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBonesPresentConditionComponent : Component
{
    [DataField]
    public bool Inverse;
}
