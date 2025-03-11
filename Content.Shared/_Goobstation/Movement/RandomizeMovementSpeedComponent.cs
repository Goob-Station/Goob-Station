namespace Content.Shared._Goobstation.Movement;

[RegisterComponent]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// How low the movement speed can drop.
    /// </summary>

    [DataField]
    public float Min { get; set; } = 1;

    /// <summary>
    /// How high the movement speed can go
    /// </summary>

    [DataField]
    public float Max { get; set; } = 1;

    /// <summary>
    /// How high the movement speed can go
    /// </summary>
    [DataField]
    public TimeSpan? NextSpeedChange;

}
