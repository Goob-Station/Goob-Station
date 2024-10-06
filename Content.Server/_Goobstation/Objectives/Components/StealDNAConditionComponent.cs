using Content.Server._Goobstation.Changeling;
using Content.Server._Goobstation.Objectives.Systems;

namespace Content.Server._Goobstation.Objectives.Components;

[RegisterComponent, Access(typeof(ChangelingObjectiveSystem), typeof(ChangelingSystem))]
public sealed partial class StealDNAConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DNAStolen = 0f;
}
