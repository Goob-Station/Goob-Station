using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// 
/// </summary>

[RegisterComponent]
public sealed partial class SegmentedBeamComponent : Component
{
    /// <summary>
    /// As this component is attached to the BeamStart entity anyway, this is only used to keep track of which entity started the beam.
    /// </summary>
    [DataField]
    public EntProtoId BeamStart;

    /// <summary>
    /// 
    /// </summary>
    [DataField]
    public EntProtoId BeamMiddle;

    /// <summary>
    /// 
    /// </summary>
    [DataField]
    public EntProtoId BeamEnd;

    /// <summary>
    /// 
    /// </summary>
    [DataField]
    public int SegmentCount;

    /// <summary>
    /// 
    /// </summary>
    [DataField]
    public float SegmentSpacing;

    /// <summary>
    /// 
    /// </summary>
    [DataField]
    public List<EntityUid> SpawnedSegments = new();

    /// <summary>
    /// If a target is set, the lasers will spawn in facing this target and shoot straight for them.
    /// </summary>
    [DataField]
    public EntityUid? Target;
}
