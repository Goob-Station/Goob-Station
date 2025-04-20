using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBleedsPresentConditionComponent : Component
{
    [DataField]
    public bool Inverted = false;
}