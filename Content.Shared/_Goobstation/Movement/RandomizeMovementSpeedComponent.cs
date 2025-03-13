namespace Content.Shared._Goobstation.Movement;

[RegisterComponent]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// How low the movement speed can drop.
    /// </summary>

    [DataField]
    public float Min { get; set; } = 2f;

    /// <summary>
    /// How high the movement speed can go
    /// </summary>

    [DataField]
    public float Max { get; set; } = 4f;

    /// <summary>
    /// bweh
    /// </summary>
    [DataField]
    public TimeSpan? NextSpeedChange;

    [DataField]
    public float CurrentModifier { get; set; } = 1f;

}
