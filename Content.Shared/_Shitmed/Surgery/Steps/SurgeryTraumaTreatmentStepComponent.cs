using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Steps;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTraumaTreatmentStepComponent : Component
{
    [DataField]
    public ProtoId<TraumaTypePrototype> TraumaType = "BoneDamage";

    [DataField]
    public FixedPoint2 Amount = 5;
}
