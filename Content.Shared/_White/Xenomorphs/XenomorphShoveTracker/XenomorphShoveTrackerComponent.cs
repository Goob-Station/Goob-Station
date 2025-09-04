using Robust.Shared.GameStates;

namespace Content.Shared._White.Xenomorphs.XenomorphShoveTracker;

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphShoveTrackerComponent : Component
{
    /// <summary>
    /// Dictionary tracking shove counts per target entity
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, int> ShoveCount = new();

    /// <summary>
    /// Number of shoves required to knock down a target
    /// </summary>
    [DataField]
    public int ShoveThreshold = 3;

    /// <summary>
    /// Duration of the knockdown effect
    /// </summary>
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Time after which shove counts reset if no new shoves occur
    /// </summary>
    [DataField]
    public TimeSpan ShoveResetTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Dictionary tracking when each target was last shoved
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, TimeSpan> LastShoveTime = new();
}
