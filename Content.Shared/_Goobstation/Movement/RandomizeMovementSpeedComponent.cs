using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Goobstation.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
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

    /// <summary>
    /// Time until next randomization.
    /// </summary>
    [DataField("nextRandomize", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan NextRandomize = TimeSpan.Zero;

}
