using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Null Charge ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingNullChargeComponent : Component
{
    public string? NullChargeAction = "ActionNullCharge";

    [DataField]
    public TimeSpan NullChargeToComplete = TimeSpan.FromSeconds(10);

    [DataField]
    public float Range = 1f;

    [DataField]
    public string? NullChargeEffect = "ShadowlingNullChargeEffect";
}
