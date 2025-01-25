namespace Content.Shared.SelfExtinguisher;

/// <summary>
///     Used to refill the charges of self-extinguishers.
/// </summary>
[RegisterComponent]
public sealed partial class SelfExtinguisherRefillComponent : Component
{
    // <summary>
    //   The amount of charges to refill.
    // </summary>
    [DataField(required: true)]
    public int RefillAmount;
}
