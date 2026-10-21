namespace Content.Server._Goobstation.MaintsSounds;

/// <summary>
///     Marks an entity as part of the maintenance area.
///     A player standing near any entity with this component is considered "in maints".
/// </summary>
[RegisterComponent]
public sealed partial class MaintsAreaMarkerComponent : Component;

/// <summary>
///     Per-player timer for the next ambient maints sound.
///     Added on demand by <see cref="MaintsSoundsSystem"/>.
/// </summary>
[RegisterComponent]
public sealed partial class MaintsSoundsListenerComponent : Component
{
    /// <summary>
    ///     Game time at which this player may next hear a maints sound.
    /// </summary>
    [DataField]
    public TimeSpan NextSound;
}
