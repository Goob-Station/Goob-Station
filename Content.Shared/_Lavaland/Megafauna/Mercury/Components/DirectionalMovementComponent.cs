using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Moves an entity towards a direction.
/// </summary>

[RegisterComponent]
public sealed partial class DirectionalMovementComponent : Component
{
    /// <summary>
    /// Direction to move towards. Declare it with X and Y in YAML, look for how it is used in MoveUp/MoveDown (etc) prototypes for reference if need be.
    /// </summary>
    [DataField]
    public Vector2 Direction = Vector2.Zero;

    /// <summary>
    /// Speed at which to move towards that direction.
    /// </summary>
    [DataField]
    public float Speed = 10f;
    public float CurrentSpeed;

    /// <summary>
    /// Optional acceleration. If zero, full speed from get go. Put any number for otherwise.
    /// </summary>
    [DataField]
    public float Acceleration;
    public TimeSpan NextUpdate;

}
