using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Displays a cinematic cutscene broken up into segments.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CinematicComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CinematicPrototype>? Timeline;

    [DataField, AutoNetworkedField]
    public TimeSpan StartTime;

    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

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
    public bool Engaged;

    [DataField]
    public float Strength;

    [DataField]
    public Vector2 Pan;

    [DataField]
    public Vector2 EyeOffset;

    [DataField]
    public bool RegistryApplied;

    [DataField]
    public int ActiveSegment = -1;

    [DataField]
    public ComponentRegistry? Overridden;

    [DataField]
    public List<EntityUid> SegmentSounds = new();

    /// <summary>
    /// Sound sources the timeline has ducked.
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, DuckedVolume> Ducked = new();
}

/// <summary>
/// The volume a sound had before ducking and the volume it was ducked to.
/// </summary>
public readonly record struct DuckedVolume(float Original, float Ducked);
