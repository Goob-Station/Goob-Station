using Robust.Shared.GameStates;

namespace Content.Shared.Standing;

/// <summary>
/// When present on an entity, standing up should not require a BodyComponent or any legs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IgnoreLegsForStandingComponent : Component
{
}
