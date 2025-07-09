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
    public float speedModWalk = 1.5f;

    /// <summary>
    /// The speed modifier the entity gets once it stands on blood.
    /// </summary>
    [DataField]
    public float speedModRun = 1.5f;

    /// <summary>
    /// This indicates whether the entity is standing on blood, or not. Used for speed modifiers
    /// </summary>
    [DataField]
    public bool IsOnBlood;

    [DataField]
    public TimeSpan Accumulator = TimeSpan.Zero;

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.FromSeconds(0.5f);
}
