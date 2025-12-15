using Content.Goobstation.Shared.Insurance;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Insurance;

public sealed partial class InsurancePolicySystem : SharedInsurancePolicySystem
{
    public override void Insure(EntityUid target, EntityUid owner, List<EntProtoId> compensationItems) { }
}
