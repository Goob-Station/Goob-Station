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
    public int Fallen = 0;
    /// <summary>
    /// amount of material needed for a new bingle
    /// </summary>
    [DataField]
    public int SpawnNewAt = 10;

    /// <summary>
    /// amount bingles needed to evolve / widen the pit / expand the ... THE FACTORY MUST GROW
    /// </summary>
    [DataField]
    public int UpgradeLimit = 12;

    [DataField]
    public bool SwallowMobs = false;

    /// <summary>
    /// Where the entities go when it falls into the pit, empties when it is destroyed.
    /// </summary>
    [DataField]
    public Container Pit = default!;
}
