using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Animates a point light's rotation while enabled.
/// All animation is done in the client system.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedRotatingLightSystem))]
public sealed partial class RotatingLightComponent : Component
{
    /// <summary>
    /// Speed to rotate at, in degrees per second
    /// </summary>
    [DataField("speed")]
    public float Speed = 90f;

    /// <summary>
    /// Goobstation.
    /// if not null, speed is randomized between <see cref="Speed"/> and this.
    /// </summary>
    [DataField]
    public float? MaxSpeed;

    [DataField]
    public bool RandomizeDirection;

    [ViewVariables(VVAccess.ReadOnly)]
    public int Direction = 1;

    [ViewVariables, AutoNetworkedField]
    public bool Enabled = true;
}
