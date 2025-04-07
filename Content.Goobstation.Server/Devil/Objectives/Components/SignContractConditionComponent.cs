using Content.Goobstation.Server.Devil.Objectives.Systems;

namespace Content.Goobstation.Server.Devil.Objectives.Components;

[RegisterComponent, Access(typeof(DevilSystem), typeof(DevilObjectiveSystem))]

public sealed partial class SignContractConditionComponent : Component
{
    [DataField]
    public int ContractsSigned = 0;
}
