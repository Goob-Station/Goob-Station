using Content.Shared.FixedPoint;

namespace Content.Server.Medical.Components;

[RegisterComponent]
[AutoGenerateComponentState]
public sealed partial class MedicalPatchComponent : Component
{
    [DataField]
    public string SolutionName = "drink";
    [DataField] // [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 TransferAmount = FixedPoint2.New(1);
    [DataField]
    public bool SingelUse = false;
    [DataField]
    public float UpdateTime = 1f;
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
    [DataField]
    public FixedPoint2 InjectAmmountOnAttatch = FixedPoint2.New(0);
    [DataField]
    public FixedPoint2 InjectPercentageOnAttatch = FixedPoint2.New(0);
}


