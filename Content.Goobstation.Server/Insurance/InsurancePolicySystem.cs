using Content.Goobstation.Shared.Insurance;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Insurance;

public sealed partial class InsurancePolicySystem : SharedInsurancePolicySystem
{
    public override void Insure(EntityUid target, EntityUid owner, List<EntProtoId> compensationItems)
    {
        InsuranceComponent insurance = EnsureComp<InsuranceComponent>(target);
        insurance.PolicyOwner = owner;
        insurance.CompensationItems = compensationItems;
    }
}
