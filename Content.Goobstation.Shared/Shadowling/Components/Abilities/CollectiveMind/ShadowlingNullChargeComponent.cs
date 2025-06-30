using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Null Charge ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingNullChargeComponent : Component
{
    [DataField]
    public EntProtoId NullChargeAction = "ActionNullCharge";

    [DataField]
    public TimeSpan NullChargeToComplete = TimeSpan.FromSeconds(10);

    [DataField]
    public float Range = 1f;

    [DataField]
    public EntProtoId NullChargeEffect = "ShadowlingNullChargeEffect";
}
