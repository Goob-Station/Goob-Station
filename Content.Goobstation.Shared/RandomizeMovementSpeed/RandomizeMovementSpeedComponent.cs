using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.RandomizeMovementSpeed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RandomizeMovementspeedComponent : Component
{
    /// <summary>
    /// The minimum limit of the modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Min { get; set; } = 0.6f;

    /// <summary>
    /// The max limit of the modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Max { get; set; } = 1.6f;

    /// <summary>
    /// The current modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentModifier { get; set; } = 1f;

    /// <summary>
    /// The Uid of the entity that picked up the item.
    /// </summary>
    [DataField]
    public EntityUid EntityUid = default!;

}
