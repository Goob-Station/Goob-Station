using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SlaughterDemon;

[RegisterComponent, NetworkedComponent]
public sealed partial class SlaughterDemonComponent : Component
{
    /// <summary>
    /// The list of mobs that the entity has devoured/consumed.
    /// </summary>
    [DataField]
    public List<EntityUid> ConsumedMobs { get; set; } = new();

    /// <summary>
    /// The number of devoured mobs.
    /// </summary>
    [DataField]
    public int Devoured;

    /// <summary>
    /// The walk modifier the entity gets once it stands on blood.
    /// </summary>
    [DataField]
    public float SpeedModWalk = 3f;

    /// <summary>
    /// The speed modifier the entity gets once it stands on blood.
    /// </summary>
    [DataField]
    public float SpeedModRun = 3f;

    /// <summary>
    /// This indicates whether the entity exited blood crawl
    /// </summary>
    [DataField]
    public bool ExitedBloodCrawl;

    /// <summary>
    /// The accumulator for when a Slaughter Demon exits blood crawl
    /// </summary>
    [DataField]
    public TimeSpan Accumulator = TimeSpan.Zero;

    /// <summary>
    /// How long the speed boost lasts after a Slaughter Demon exits blood crawl
    /// </summary>
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.FromSeconds(6f);
}
