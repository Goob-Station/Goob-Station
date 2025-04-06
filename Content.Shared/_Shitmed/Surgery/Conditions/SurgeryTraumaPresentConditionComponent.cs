using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared.Body.Part;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTraumaPresentConditionComponent : Component
{
    [DataField("trauma")]
    public TraumaType TraumaType = TraumaType.BoneDamage;

    [DataField]
    public bool Inverted = false;
}