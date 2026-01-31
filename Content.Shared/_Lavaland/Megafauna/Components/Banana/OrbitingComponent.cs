[RegisterComponent]
public sealed partial class OrbitingComponent : Component
{
    /// <summary>
    /// The owner to orbit around.
    /// </summary>
    public EntityUid Owner;

    /// <summary>
    /// The angle at which it orbits.
    /// </summary>
    public float Angle;

    /// <summary>
    /// Current distance from owner.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Maximum distance from owner.
    /// </summary>
    public float MaxRadius;

    /// <summary>
    /// How quickly they expand outwards.
    /// </summary>
    public float GrowSpeed;

    /// <summary>
    /// Speed at which it rotates.
    /// </summary>
    public float RotationSpeed = MathF.Tau; // 1 full rotation per second
}
