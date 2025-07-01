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
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    ///  Indicates if the user has moved since the last time.
    /// </summary>
    [DataField]
    public bool IsUserActive;

    /// <summary>
    ///  The last known position of the user of this component
    /// </summary>
    [DataField]
    public Vector2 LastKnownPosition;
}
