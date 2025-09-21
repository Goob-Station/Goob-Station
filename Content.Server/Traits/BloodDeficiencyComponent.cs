namespace Content.Server.Traits.Assorted;

/// <summary>
///     This is used for the Blood Deficiency trait.
/// </summary>
[RegisterComponent]
public sealed partial class BloodDeficiencyComponent : Component
{
    // <summary>
    ///     How much percentage of max blood volume should be removed in each update interval?
    // </summary>
    [DataField(required: true)]
    public float BloodLossPercentage;

    /// <summary>
    ///     Whether the effects of this trait should be active.
    /// </summary>
    [DataField]
    public bool Active = true;
}
