using System.Numerics;


namespace Content.Shared._EE.Shadowling.Components;


/// <summary>
/// This is used for detecting if an entity is near a lighted area
/// </summary>
[RegisterComponent]
public sealed partial class LightDetectionComponent : Component
{
    /// <summary>
    ///  Is user standing on a lighted area?
    /// </summary>
    [DataField]
    public bool IsOnLight;

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.25);

    /// <summary>
    ///  Has the user moved since the last check?
    /// </summary>
    [DataField]
    public bool IsUserActive;

    [DataField]
    public Vector2 LastKnownPosition = new Vector2();
}
