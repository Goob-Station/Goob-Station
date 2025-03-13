using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Server.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MovementSpeedModifierSystem))]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// The minimum limit of the modifier.
    /// </summary>

    [DataField]
    public float Min { get; set; } = 0.6f;

    /// <summary>
    /// The max limit of the modifier.
    /// </summary>

    [DataField]
    public float Max { get; set; } = 1.6f;

    /// <summary>
    /// The current modifier.
    /// </summary>
    [DataField]
    public float CurrentModifier { get; set; } = 1f;

}
