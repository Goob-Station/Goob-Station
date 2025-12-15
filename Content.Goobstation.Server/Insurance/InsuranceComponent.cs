using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Insurance;

[RegisterComponent]
public sealed partial class InsuranceComponent : Component
{
    [DataField]
    public EntityUid PolicyOwner;

    [DataField]
    public List<EntProtoId> CompensationItems;
}
