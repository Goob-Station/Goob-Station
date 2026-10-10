using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Gives the pilot these components while they're inside the mech and takes them away when they leave.
/// </summary>
[RegisterComponent]
public sealed partial class GiveMechPilotComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    [ViewVariables]
    public List<string> Added = new();
}
