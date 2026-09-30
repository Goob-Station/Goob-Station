using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
/// Surgery is valid while the part carries any surgically-treatable trauma.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTraumaTreatableConditionComponent : Component
{
    [DataField]
    public bool Inverted;
}
