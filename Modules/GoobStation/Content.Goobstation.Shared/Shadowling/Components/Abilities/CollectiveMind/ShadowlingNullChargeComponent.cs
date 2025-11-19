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
    public EntProtoId ActionId = "ActionNullCharge";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// The seconds it takes for the null charge ability to complete.
    /// </summary>
    [DataField]
    public TimeSpan NullChargeToComplete = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The search radius of this ability.
    /// </summary>
    [DataField]
    public float Range = 1f;

    /// <summary>
    /// The effect that is used once the ability activates.
    /// </summary>
    [DataField]
    public EntProtoId NullChargeEffect = "ShadowlingNullChargeEffect";
}
