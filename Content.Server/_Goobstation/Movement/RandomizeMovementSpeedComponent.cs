namespace Content.Server._Goobstation.Movement;

[RegisterComponent]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// How low the movement speed can drop.
    /// </summary>

    [DataField("Min", required: true)]
    public float Min = 1f;

    /// <summary>
    /// How high the movement speed can go
    /// </summary>

    [DataField("Max", required: true)]
    public float Max = 1f;

    /// <summary>
    /// How long between each randomization.
    /// </summary>
    [DataField("Interval", required: true)]
    public float Interval = 1f;
}
