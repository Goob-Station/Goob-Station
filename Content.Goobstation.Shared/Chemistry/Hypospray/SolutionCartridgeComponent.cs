using Content.Shared.Chemistry.Components;

namespace Content.Goobstation.Shared.Chemistry.Hypospray;

[RegisterComponent]
public sealed partial class SolutionCartridgeComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string TargetSolution = "default";

    [DataField(required: true)]
    public Solution Solution;
}
