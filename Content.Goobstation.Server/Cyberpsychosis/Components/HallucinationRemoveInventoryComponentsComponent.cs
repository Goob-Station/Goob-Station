using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cyberpsychosis;

/// <summary>
/// Component used to store a list of components to be removed from hallucination's inventory contents.
/// </summary>
[RegisterComponent]
public sealed partial class HallucinationRemoveInventoryComponentsComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
