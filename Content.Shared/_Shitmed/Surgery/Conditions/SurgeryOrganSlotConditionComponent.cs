using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
/// Requires that an organ slot does (not) exist on the target part for a surgery to be possible.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryOrganSlotConditionComponent : Component
{
    [DataField(required: true)]
    public string OrganSlot = default!;

    [DataField]
    public bool Inverse;
}
