namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Moves an entity towards a direction. Acceleration does not come into play.
/// </summary>

[RegisterComponent]
public sealed partial class DirectionalMovementComponent : Component
{
    /// <summary>
    /// Direction to move towards. Not mutually exclusive so don't be dumb.
    /// </summary>
    [DataField]
    public bool MoveNorth;
    [DataField]
    public bool MoveSouth;
    [DataField]
    public bool MoveWest;
    [DataField]
    public bool MoveEast;

    /// <summary>
    /// Speed at which to move towards that direction. Also acts as the speed cap when Acceleration is set.
    /// </summary>
    [DataField]
    public float Speed = 10f;
    public float CurrentSpeed;

    /// <summary>
    /// Optional acceleration. If zero, full speed from get go. Put any number for otherwise.
    /// </summary>
    [DataField]
    public float Acceleration;

}
