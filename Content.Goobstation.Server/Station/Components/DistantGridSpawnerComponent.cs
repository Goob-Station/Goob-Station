using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Station.Components;

/// <summary>
/// Attempts to spawn the grid in deep space. Any grid that has this component will attempt to spawn in during round loading.
/// This system is intended for rare spawning of grids, so it is preferable for you to set the odds of the grid spawning to be fairly low.
/// Random debris might make this code pretty rare to work, dunno, depends on how many debri the game decides to spawn I suppose.
/// </summary>

[RegisterComponent]
public sealed partial class DistantGridSpawnerComponent : Component
{
    [DataField(required: true)]
    public List<DistantGridEntry> Entries = new();
}

[DataDefinition]
public sealed partial class DistantGridEntry
{
    /// <summary>
    /// Prototype path for the grid map file.
    /// </summary>
    [DataField(required: true)]
    public ResPath Path;

    /// <summary>
    /// Chance for the grid to spawn in.
    /// </summary>
    [DataField]
    public float Chance = 0.1f;

    /// <summary>
    /// Minimal distance from the station at which this grid may spawn.
    /// </summary>
    [DataField]
    public float MinDistance = 1000f;

    /// <summary>
    /// Maximum distance from the station at which this grid may spawn.
    /// </summary>
    [DataField]
    public float MaxDistance = 1500f;

    /// <summary>
    /// How much empty space is required around the selected spawning point.
    /// Larger values will lead to more failed attempts.
    /// This value should, however, be large if your grid is large,
    /// otherwise it may lead to the grid spawning inside other grids.
    /// </summary>
    [DataField]
    public float SafetyRadius = 50f;
}
