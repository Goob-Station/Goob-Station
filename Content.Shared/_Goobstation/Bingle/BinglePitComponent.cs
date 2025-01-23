using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent]
public sealed partial class BinglePitComponent : Component
{
    /// <summary>
    /// ammount of stored
    /// </summary>
    [DataField]
    public float BinglePoints = 0f;
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
    /// if the Bingle pit level
    /// </summary>
    [DataField]
    public float Level = 0f;
    /// <summary>
    /// Where the entities go when it falls into the pit, empties when it is destroyed.
    /// </summary>
    public Container Pit = default!;
    [DataField]
    public float MaxSize = 3f;
    [DataField]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");
}

[Serializable, NetSerializable]
public sealed class BinglePitGrowEvent(NetEntity uid, float level) : EntityEventArgs
{
    public NetEntity Uid = uid;
    public float Level = level;
}
