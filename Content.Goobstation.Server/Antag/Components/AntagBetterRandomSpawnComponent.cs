using Robust.Shared.Map;

namespace Content.Goobstation.Server.Antag.Components;

/// <summary>
/// Spawns this rule's antags at random tiles on a station,
/// but with additional safety checks to prevent spawning in camera line of sight, in space, or inside solid objects.
/// Psuedo forces them to spawn in maints and unpopulated area's.
/// Requires AntagSelectionComponent.
/// </summary>
[RegisterComponent]
public sealed partial class AntagBetterRandomSpawnComponent : Component
{
    /// <summary>
    /// Location that was picked.
    /// </summary>
    [DataField]
    public EntityCoordinates? Coords;

    /// <summary>
    /// Range to check for camera line of sight.
    /// </summary>
    [DataField]
    public float CameraCheckRange = 15f;

    /// <summary>
    /// Maximum number of attempts.
    /// </summary>
    [DataField]
    public int MaxAttempts = 2000;
}
