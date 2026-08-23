using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Displays a cinematic cutscene broken up into sequences.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CinematicComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan StartTime;

    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    /// <summary>
    /// Ease in time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float IntroTime = 0.35f;

    [DataField, AutoNetworkedField]
    public float OutroTime = 1.1f;

    [DataField, AutoNetworkedField]
    public float EngageRate = 22f;

    [DataField, AutoNetworkedField]
    public float DisengageRate = 10f;

    /// <summary>
    /// How fast the camera pan eases back.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PanReturnRate = 8f;

    /// <summary>
    /// pulls other players camera towards whoever has the cinematic playing on them.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool PullsOtherCameras;

    /// <summary>
    /// How far away a player can be pulled into the cinematic. Line of sight is also required.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ViewerRange = 14f;

    /// <summary>
    /// How far along the line from the viewer to the focus the camera travels (1 = all the way onto them).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CameraPull = 1f;

    /// <summary>
    /// Safety cap on how far the camera may travel, in tiles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxPanDistance = 12f;

    [DataField]
    public float Strength;

    [DataField]
    public Vector2 Pan;

    [DataField]
    public bool Engaged;

    [DataField]
    public Vector2 EyeOffset;

    #region Timeline

    /// <summary>
    /// The scripted timeline prototype.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<CinematicPrototype>? Timeline;

    /// <summary>
    /// Fills {$name} in segment captions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? SubjectName;

    /// <summary>
    /// Fills {$station} in segment captions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? StationName;

    [DataField]
    public bool RegistryApplied;

    [DataField]
    public int ActiveSegment = -1;

    [DataField]
    public List<EntityUid> SegmentSounds = new();

    /// <summary>
    /// Sound sources the timeline has ducked.
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, float> Ducked = new();

    #endregion
}
