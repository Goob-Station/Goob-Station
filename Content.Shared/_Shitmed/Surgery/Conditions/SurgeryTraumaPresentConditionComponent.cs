using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTraumaPresentConditionComponent : Component
{
    [DataField("trauma")]
    public ProtoId<TraumaTypePrototype> TraumaType = "BoneDamage";

    [DataField]
    public bool Inverted = false;
}