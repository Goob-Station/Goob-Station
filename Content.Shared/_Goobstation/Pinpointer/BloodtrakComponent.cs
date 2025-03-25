using Content.Shared._Gobostation.Pinpointer;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Pinpointer;

/// <summary>
/// Allows an item to track another entity based on DNA from a solution.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedBloodtrakSystem))]
public sealed partial class BloodtrakComponent : Component
{
    /// <summary>
    /// The duration the tracker will remain on, before shutting off automatically.
    /// </summary>
    [DataField]
    public TimeSpan TrackingDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The distance defined as being a medium distance away.
    /// </summary>
    [DataField]
    public float MediumDistance = 16f;

    /// <summary>
    /// The distance defined as being a short distance away.
    /// </summary>
    [DataField]
    public float CloseDistance = 8f;

    /// <summary>
    /// The distance defined as being close.
    /// </summary>
    [DataField]
    public float ReachedDistance = 1f;

    /// <summary>
    ///     Pinpointer arrow precision in radians.
    /// </summary>
    [DataField]
    public double Precision = 0.09;

    /// <summary>
    /// The current target of the tracker.
    /// </summary>
    [ViewVariables]
    public EntityUid? Target = null;

    /// <summary>
    /// Whether the tracker is currently active.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsActive = false;

    /// <summary>
    /// The current angle of the trackers arrow.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Angle ArrowAngle;

    /// <summary>
    /// The current distance to the target.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Distance DistanceToTarget = Distance.Unknown;

    /// <summary>
    /// How long until the next execution.
    /// </summary>
    [ViewVariables]
    public TimeSpan NextExecutionTime = TimeSpan.FromSeconds(30);

    [ViewVariables]
    public bool HasTarget => DistanceToTarget != Distance.Unknown;

}

[Serializable, NetSerializable]
public enum Distance : byte
{
    Unknown,
    Reached,
    Close,
    Medium,
    Far
}
