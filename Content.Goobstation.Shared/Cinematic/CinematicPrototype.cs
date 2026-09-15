using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// A scripted cinematic.
/// </summary>
[Prototype]
public sealed partial class CinematicPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<CinematicSegment> Segments = new();

    /// <summary>
    /// Components applied to the focus entity for the whole timeline.
    /// A segment can add a component of the same type to replace it for that segment only.
    /// </summary>
    [DataField]
    public ComponentRegistry? AddComp;

    #region Timing

    /// <summary>
    /// Seconds the cinematic takes to ease in.
    /// </summary>
    [DataField]
    public float IntroTime = 0.35f;

    /// <summary>
    /// Seconds the cinematic takes to ease out.
    /// </summary>
    [DataField]
    public float OutroTime = 1.1f;

    [DataField]
    public float EngageRate = 22f;

    [DataField]
    public float DisengageRate = 10f;

    #endregion

    #region Audio

    /// <summary>
    /// Makes all audio sources outside of ones added by the cinematic have lower volume.
    /// </summary>
    [DataField]
    public bool DuckAudio = true;

    /// <summary>
    /// How many decibels everything else is lowered by.
    /// </summary>
    [DataField]
    public float DuckDecibels = 24f;

    #endregion

    #region Camera

    /// <summary>
    /// Pulls other nearby players cameras towards whoever has the cinematic playing on them.
    /// </summary>
    [DataField]
    public bool PullsOtherCameras;

    /// <summary>
    /// How far away a player can be pulled into the cinematic. Line of sight is also required.
    /// </summary>
    [DataField]
    public float ViewerRange = 14f;

    /// <summary>
    /// How far along the line from the viewer to the focus the camera travels (1 = all the way onto them).
    /// </summary>
    [DataField]
    public float CameraPull = 1f;

    /// <summary>
    /// Safety cap on how far the camera may travel.
    /// </summary>
    [DataField]
    public float MaxPanDistance = 12f;

    /// <summary>
    /// How fast the camera pan eases back.
    /// </summary>
    [DataField]
    public float PanReturnRate = 8f;

    #endregion
}

[DataDefinition]
public sealed partial class CinematicSegment
{
    /// <summary>
    /// Seconds this segment runs for.
    /// </summary>
    [DataField(required: true)]
    public float Duration;

    /// <summary>
    /// Played for everyone watching as the segment begins.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Components added to the focus entity for this segments duration and removed with it.
    /// (Cinematic zoom, cinematic letterbox, etc).
    /// </summary>
    [DataField]
    public ComponentRegistry? AddComp;

    /// <summary>
    /// Events raised on the focus entity as the segment begins.
    /// </summary>
    [DataField]
    public List<object> Events = new();
}
