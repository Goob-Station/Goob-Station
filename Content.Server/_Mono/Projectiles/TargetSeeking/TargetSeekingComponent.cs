using System.Numerics;

namespace Content.Server._Mono.Projectiles.TargetSeeking;

/// <summary>
/// Component that allows a projectile to seek and track targets autonomously.
/// </summary>
[RegisterComponent]
public sealed partial class TargetSeekingComponent : Component
{
    /// <summary>
    /// Maximum distance to search for potential targets.
    /// </summary>
    [DataField]
    public float DetectionRange = 300f;

    /// <summary>
    /// Angular range in which targets can be detected and tracked.
    /// </summary>
    [DataField]
    public Angle ScanArc = Angle.FromDegrees(360);

    /// <summary>
    /// How quickly the projectile can change direction in degrees per second.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle? TurnRate = 100f;

    /// <summary>
    /// The current target entity being tracked.
    /// </summary>
    [DataField]
    public EntityUid? CurrentTarget;

    /// <summary>
    /// Tracking algorithm used for intercepting the target.
    /// </summary>
    [DataField]
    public TrackingMethod TrackingAlgorithm = TrackingMethod.Predictive;

    /// <summary>
    /// How fast the projectile accelerates in m/s².
    /// </summary>
    [DataField]
    public float Acceleration = 50f;

    /// <summary>
    /// Maximum speed the projectile can reach in m/s.
    /// </summary>
    [DataField]
    public float MaxSpeed = 50f;

    /// <summary>
    /// Initial speed of the projectile in m/s.
    /// </summary>
    [DataField]
    public float LaunchSpeed = 10f;

    /// <summary>
    /// Current speed of the projectile in m/s.
    /// </summary>
    [DataField]
    public float CurrentSpeed;

    /// <summary>
    /// Field of view in degrees for target detection.
    /// </summary>
    [DataField]
    public float FieldOfView = 90f;

    /// <summary>
    /// Used for tracking metrics between updates.
    /// </summary>
    public float PreviousDistance;

    /// <summary>
    /// Previous position of the target, used for velocity calculation.
    /// </summary>
    public Vector2 PreviousTargetPosition;
}

/// <summary>
/// Defines different tracking algorithms that can be used.
/// </summary>
public enum TrackingMethod
{
    /// <summary>
    /// Advanced tracking that predicts target movement.
    /// </summary>
    Predictive = 1,

    /// <summary>
    /// Basic tracking that simply points directly at the target.
    /// </summary>
    Direct = 2
}
