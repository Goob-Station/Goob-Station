namespace Content.Shared._Goobstation.Movement;

[RegisterComponent]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// How low the movement speed can drop.
    /// </summary>

    [DataField]
    public float Min { get; set; } = 0.6f;

    /// <summary>
    /// How high the movement speed can go
    /// </summary>

    [DataField]
    public float Max { get; set; } = 2f;

    /// <summary>
    /// bweh
    /// </summary>
    [DataField]
    public TimeSpan? NextSpeedChange;

}
