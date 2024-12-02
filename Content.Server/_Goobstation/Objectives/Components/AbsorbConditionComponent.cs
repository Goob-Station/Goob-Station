using Content.Server._Goobstation.Changeling;
using Content.Server._Goobstation.Objectives.Systems;

namespace Content.Server._Goobstation.Objectives.Components;

[RegisterComponent]
public sealed partial class AbsorbConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Absorbed = 0f;
}
