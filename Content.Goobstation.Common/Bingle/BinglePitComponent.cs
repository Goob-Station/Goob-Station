// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.Bingle;

[RegisterComponent]
public sealed partial class BinglePitComponent : Component
{
    /// <summary>
    /// ammount of stored
    /// </summary>
    [DataField]
    public float BinglePoints = 0f;

    [DataField]
    public float PointsForAlive = 5f;

    [DataField]
    public float AdditionalPointsForHuman = 5f;

    /// <summary>
    /// amount of Bingle Points needed for a new bingle
    /// </summary>
    [DataField]
    public float SpawnNewAt = 12f;

    /// <summary>
    /// amount bingles needed to evolve / gain a level / expand the ... THE FACTORY MUST GROW
    /// </summary>
    [DataField]
    public float MinionsMade = 0f;

    [DataField]
    public float UpgradeMinionsAfter = 10f;

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

    [DataField]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");

    [DataField]
    public EntProtoId GhostRoleToSpawn = "SpawnPointGhostBingle";

    /// <summary>
    /// Rare skin variant of <see cref="GhostRoleToSpawn"/>.
    /// </summary>
    [DataField]
    public EntProtoId RareGhostRoleToSpawn = "SpawnPointGhostTurkle";

    /// <summary>
    /// Chance to spawn <see cref="RareGhostRoleToSpawn"/> instead of <see cref="GhostRoleToSpawn"/>.
    /// 1/50, matching the weights the old MobBingleRandom entity table used.
    /// </summary>
    [DataField]
    public float RareGhostRoleChance = 0.02f;

    /// <summary>
    /// how many bingles to spawn on pit spawn
    /// </summary>
    [DataField]
    public int StartingBingles = 3;

    /// <summary>
    /// Bunch of available bingle ghost role
    /// </summary>
    public HashSet<EntityUid?> BingleGhostRoles = [];
}
