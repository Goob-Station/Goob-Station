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

    [ViewVariables]
    public bool Engaged;

    [ViewVariables]
    public float Strength;

    [ViewVariables]
    public Vector2 Pan;

    [ViewVariables]
    public Vector2 EyeOffset;

    [ViewVariables]
    public bool RegistryApplied;

    [ViewVariables]
    public int ActiveSegment = -1;

    [ViewVariables]
    public ComponentRegistry? Overridden;

    [ViewVariables]
    public List<EntityUid> SegmentSounds = new();

    /// <summary>
    /// Sound sources the timeline has ducked.
    /// </summary>
    [ViewVariables]
    public Dictionary<EntityUid, DuckedVolume> Ducked = new();
}

/// <summary>
/// The volume a sound had before ducking and the volume it was ducked to.
/// </summary>
public readonly record struct DuckedVolume(float Original, float Ducked);
