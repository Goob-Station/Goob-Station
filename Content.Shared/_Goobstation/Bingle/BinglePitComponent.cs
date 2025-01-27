using Robust.Shared.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent]
public sealed partial class BinglePitComponent : Component
{
    /// <summary>
    /// ammount of stored
    /// </summary>
    [DataField]
    public float BinglePoints = 0f;
    public float PointsForAlive = 5f;
    public float AdditionalPointsForHuman = 5f;
    /// <summary>
    /// amount of Bingle Points needed for a new bingle
    /// </summary>
    [DataField]
    public float SpawnNewAt = 10f;

    /// <summary>
    /// amount bingles needed to evolve / gain a level / expand the ... THE FACTORY MUST GROW
    /// </summary>
    [DataField]
    public float MinionsMade = 0f;

    [DataField]
    public float UpgradeMinionsAfter = 12f;

    /// <summary>
    /// the Bingle pit's level
    /// </summary>
    [DataField]
    public float Level = 1f;
    /// <summary>
    /// Where the entities go when it falls into the pit, empties when it is destroyed.
    /// </summary>
    public Container Pit = default!;
    [DataField]
    public float MaxSize = 3f;
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");
    public EntProtoId GhostRoleToSpawn = "SpawnPointGhostBingle";
}
