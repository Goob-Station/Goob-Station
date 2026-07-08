// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryPartConditionComponent : Component
{
    [DataField]
    public HashSet<BodyPartType> Parts;

    [DataField]
    public BodyPartSymmetry? Symmetry;

    [DataField]
    public bool Inverse;
}
