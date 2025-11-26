using EntityCoordinates = Robust.Shared.Map.EntityCoordinates;

namespace Content.Server._Mono.Projectiles.TargetGuided;

/// <summary>
/// Component that allows a projectile to follow the cursor position in a gunnery console.
/// </summary>
[RegisterComponent]
public sealed partial class TargetGuidedComponent : Component
{
    /// <summary>
    /// How quickly the projectile can change direction in degrees per second.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle? TurnRate = 120f;

    /// <summary>
    /// How fast the projectile accelerates in m/s².
    /// </summary>
    [DataField]
    public float Acceleration = 40f;

    /// <summary>
    /// Maximum speed the projectile can reach in m/s.
    /// </summary>
    [DataField]
    public float MaxSpeed = 20f;

    /// <summary>
    /// Initial speed of the projectile in m/s.
    /// </summary>
    [DataField]
    public float LaunchSpeed = 8f;

    /// <summary>
    /// Current speed of the projectile in m/s.
    /// </summary>
    [DataField]
    public float CurrentSpeed;

    /// <summary>
    /// The target position to guide towards.
    /// </summary>
    [DataField]
    public EntityCoordinates? TargetPosition;

    /// <summary>
    /// The console that is controlling this missile via cursor position.
    /// </summary>
    [ViewVariables]
    public EntityUid? ControllingConsole;

    /// <summary>
    /// Maximum lifetime of the projectile in seconds.
    /// </summary>
    [DataField]
    public float MaxLifetime = 30f;

    /// <summary>
    /// Current lifetime of the projectile.
    /// </summary>
    [DataField]
    public float CurrentLifetime;

    /// <summary>
    /// The entity that fired this projectile.
    /// </summary>
    [DataField]
    public EntityUid? ShooterEntity;

    /// <summary>
    /// Time since last cursor position update.
    /// </summary>
    [DataField]
    public float TimeSinceLastUpdate = 0f;

    /// <summary>
    /// Time since the cursor position has actually moved.
    /// </summary>
    [DataField]
    public float TimeSinceLastCursorMovement = 0f;

    /// <summary>
    /// Time in seconds before considering connection lost.
    /// </summary>
    [DataField]
    public float FallbackTime = 1.0f;

    /// <summary>
    /// Previous position of cursor for detecting if it has moved.
    /// </summary>
    [DataField]
    public EntityCoordinates? PreviousCursorPosition;

    /// <summary>
    /// Fixed direction the missile will maintain after losing connection.
    /// This prevents stuttering by locking the direction once connection is lost.
    /// </summary>
    [DataField]
    public Angle? FixedDirection = null;

    /// <summary>
    /// Tracks whether the missile has lost connection to its console.
    /// </summary>
    [DataField]
    public bool ConnectionLost = false;

    /// <summary>
    /// Once set to true, the missile permanently ignores any further guidance inputs.
    /// </summary>
    [DataField]
    public bool ControlPermanentlyLost = false;
}
