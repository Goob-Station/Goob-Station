using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Leash.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ProximityLeashComponent : Component
{
    /// <summary>
    /// Time accumulator for interval ticking.
    /// </summary>
    public float DamageAccumulator = 0f;

    /// <summary>
    /// Counts how many out-of-range ticks have elapsed since the leash broke.
    /// </summary>
    public int TickCounter = 0;

    /// <summary>
    /// Max allowed distance from any leash anchor before ticking begins.
    /// </summary>
    [DataField]
    public float MaxDistance = 20f;

    /// <summary>
    /// Time between each out-of-range tick.
    /// </summary>
    [DataField]
    public TimeSpan TickInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How many ticks before <see cref="ProximityLeashBreakEvent"/> is raised.
    /// Set to 0 or negative to never raise the break event.
    /// </summary>
    [DataField]
    public int BreakThreshold = 15;

    /// <summary>
    /// Only anchors with a matching LeashGroup will be considered.
    /// </summary>
    [DataField]
    public string LeashGroup = "default";
}
