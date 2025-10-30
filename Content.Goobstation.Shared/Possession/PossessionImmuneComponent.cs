using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Possession;

/// <summary>
/// Marks an entity as immune to possession attempts.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PossessionImmuneComponent : Component
{
}
