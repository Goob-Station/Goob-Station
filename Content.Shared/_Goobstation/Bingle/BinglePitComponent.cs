using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent]
public sealed partial class BinglePitComponent : Component
{
    /// <summary>
    /// ammount of stored
    /// </summary>
    [DataField]
    public float Fallen = 0f;
    /// <summary>
    /// amount of material needed for a new bingle
    /// </summary>
    [DataField]
    public float SpawnNewAt = 10f;

    /// <summary>
    /// amount bingles needed to evolve / gain a level / expand the ... THE FACTORY MUST GROW
    /// </summary>
    [DataField]
    public float MinionsMade = 0f;

    [DataField]
    public float UpgradeMinionsAfter = 12f; //changed for testing

    /// <summary>
    /// if the Bingle pit level
    /// </summary>
    [DataField]
    public float Level = 1f;

    /// <summary>
    /// Where the entities go when it falls into the pit, empties when it is destroyed.
    /// </summary>
    public Container Pit = default!;
}
