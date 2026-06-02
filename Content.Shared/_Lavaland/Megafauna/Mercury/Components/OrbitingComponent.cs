namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Assigned to entities spawned by OrbitingRing to make them, well, orbit.
/// </summary>

[RegisterComponent]
public sealed partial class OrbitingComponent : Component
{
    /// <summary>
    /// Current distance from the parent entity.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Maximum distance from the parent entity
    /// </summary>
    [DataField]
    public float MaxRadius = 2f;

    /// <summary>
    /// How quickly it expands outwards.
    /// </summary>
    [DataField]
    public float GrowSpeed = 1f;

    /// <summary>
    /// Current orbit angle in radians.
    /// </summary>
    public float Angle;
}
