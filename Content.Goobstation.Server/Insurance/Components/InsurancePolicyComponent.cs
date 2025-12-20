namespace Content.Goobstation.Server.Insurance.Components;

[RegisterComponent]
public sealed partial class InsurancePolicyComponent : Component
{
    [DataField]
    public InsurancePolicy Policy;
}
