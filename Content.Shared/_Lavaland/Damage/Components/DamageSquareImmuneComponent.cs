using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Damage.Components;

/// <summary>
/// Marker component that makes this entity immune to all damage tiles.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageSquareImmuneComponent : Component
{
    /// <summary>
    /// Time when this immunity will end and component will remove itself.
    /// </summary>
    [DataField]
    public TimeSpan? EndTime;
}
