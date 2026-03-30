using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Adds components to the entity that eats the entity this is attached to
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AddComponentsOnEatenComponent : Component
{
    [DataField]
    public ComponentRegistry Components;
}
