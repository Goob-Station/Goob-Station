using Robust.Shared.GameStates;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Components.Banana;

/// <summary>
/// Makes an entity move in a direction with optional acceleration.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DirectionalMovementComponent : Component
{
    /// <summary>
    /// The direction the entity should move in.
    /// This will be normalized at runtime.
    /// </summary>
    [DataField]
    public Vector2 Direction = Vector2.UnitX;

    /// <summary>
    /// The starting speed of the entity.
    /// </summary>
    [DataField]
    public float InitialSpeed;

    /// <summary>
    /// How quickly the speed increases over time.
    /// </summary>
    [DataField]
    public float Acceleration;

    /// <summary>
    /// Maximum allowed speed.
    /// </summary>
    [DataField]
    public float MaxSpeed = 40f;

    /// <summary>
    /// Maximum distance the entity should travel.
    /// If zero or negative, distance is ignored.
    /// </summary>
    [DataField]
    public float MaxDistance;

    /// <summary>
    /// If true, the entity will delete itself when it reaches MaxDistance.
    /// </summary>
    [DataField]
    public bool DeleteOnEnd;

    /// <summary>
    /// Current speed, tracked for acceleration.
    /// </summary>
    public float CurrentSpeed;

    /// <summary>
    /// The starting position of the movement.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Distance traveled so far.
    /// </summary>
    public float DistanceTraveled;

    /// <summary>
    /// Prevents reinitializing movement data.
    /// </summary>
    public bool Initialized;
}
