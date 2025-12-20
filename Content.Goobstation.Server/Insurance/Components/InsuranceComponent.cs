namespace Content.Goobstation.Server.Insurance.Components;

[RegisterComponent]
public sealed partial class InsuranceComponent : Component
{
    [DataField]
    public EntityUid PolicyOwner;

    [DataField]
    public InsurancePolicy Policy;
}
