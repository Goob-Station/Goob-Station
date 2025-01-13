using Robust.Shared.GameStates;

namespace Content.Shared.Turnstile;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MovementBlockerComponent : Component
{
    [DataField]
    public EntityUid Turnstile;
    public CardinalDirection CurrentDir;
}
