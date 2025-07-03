using System.Numerics;

namespace Content.Goobstation.Shared.Shadowling.Components;

/// <summary>
/// This is used for detecting if an entity is near a lighted area
/// </summary>
[RegisterComponent]
public sealed partial class LightDetectionComponent : Component
{
    /// <summary>
    ///  Indicates whether the used is standing on light, or not.
    /// </summary>
    [DataField]
    public bool IsOnLight;

    [DataField]
    public float Accumulator;

    [DataField]
    public float UpdateInterval = 1f;

    /// <summary>
    ///  The last known position of the user of this component
    /// </summary>
    [DataField]
    public Vector2 LastKnownPosition;
}
