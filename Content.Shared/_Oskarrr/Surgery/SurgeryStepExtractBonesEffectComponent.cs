// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.Surgery;

/// <summary>
/// Spawns a random amount of bone material and removes anatomical bones from the part.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryStepExtractBonesEffectComponent : Component
{
    [DataField]
    public EntProtoId Prototype = "MaterialBones1";

    [DataField]
    public int MinAmount = 1;

    [DataField]
    public int MaxAmount = 3;
}
