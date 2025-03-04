namespace Content.Server._Goobstation.Movement;

[RegisterComponent]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// How low the movement speed can drop.
    /// </summary>

    [DataField("Min", required: true)]
    public float Min { get; set; } = 1;

    /// <summary>
    /// How high the movement speed can go
    /// </summary>

    [DataField("Max", required: true)]
    public float Max { get; set; } = 1;

}
