using Robust.Shared.GameStates;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Components.Banana;

/// <summary>
/// Makes an entity spiral as it moves.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpiralingComponent : Component
{
    /// <summary>
    /// If false, the entity moves in a perfect circle at fixed radius.
    /// If true, the entity spirals outward.
    /// </summary>
    [DataField] public bool SpiralOutwards = true;

    /// <summary>
    /// How fast the entity should spiral.
    /// </summary>
    [DataField]
    public float SpiralSpeed;

    /// <summary>
    /// How far the entity should spiral.
    /// </summary>
    [DataField]
    public float SpiralDistance;

    /// <summary>
    /// How quickly the speed of the entity will ramp up.
    /// </summary>
    [DataField]
    public float SpiralAcceleration;

    /// <summary>
    /// Used to prevent the spiral from accelarating into ridiculous speeds and potentially breaking something.
    /// </summary>
    [DataField]
    public float SpiralMaxSpeed = 40f;


    /// <summary>
    /// If the entity should delete itself once it reaches SpiralDistance's range.
    /// </summary>
    [DataField]
    public bool DeleteOnEnd;

    /// <summary>
    /// The angle at which the spiral occurs.
    /// </summary>
    public float Angle;

    /// <summary>
    /// The radius of the spiralling.
    /// </summary>
    public float Radius;

    /// <summary>
    /// The current speed of the entity, tracked to make sure acceleration is working.
    /// </summary>
    public float CurrentSpeed;

    /// <summary>
    /// The center of the spiral, which will be the entity's spawning point.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Captures the origin point.
    /// </summary>
    public bool Initialized;
}
