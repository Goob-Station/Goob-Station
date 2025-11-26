using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Increases power draw on a power cell augment for this one.
/// If the augment has ItemToggleComponent then it will only apply when activated.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AugmentPowerDrawSystem))]
public sealed partial class AugmentPowerDrawComponent : Component
{
    [DataField(required: true)]
    public float Draw;
}

/// <summary>
/// Gets the total power draw of all augments for updating the power cell augment's draw.
/// Raised when any augment gets installed.
/// </summary>
[ByRefEvent]
public record struct GetAugmentsPowerDrawEvent(EntityUid Body, float TotalDraw = 0f);
