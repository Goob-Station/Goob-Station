using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.AshWalkers;

[RegisterComponent, NetworkedComponent]
public sealed partial class NecropolisNestComponent : Component
{
    /// <summary>
    /// amount of points on spawn
    /// </summary>
    [DataField]
    public float TendrilPoints = 0f;

    [DataField]
    public float AdditionalPointsForHuman = 5f;

    /// <summary>
    /// amount of points needed for a new egg
    /// </summary>
    [DataField]
    public float SpawnNewAt = 5f;

    [DataField]
    public SoundSpecifier AbsorbingSound = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg");

    [DataField]
    public EntProtoId ObjectToSpawn = "AshWalkerEgg";

    /// <summary>
    /// how many eggs to spawn on pit spawn
    /// </summary>
    [DataField]
    public int StartingEggs = 3;

    [DataField]
    public int ChasmRadius = 2;

    [DataField]
    public float ChasmDelay = 5f;
}
