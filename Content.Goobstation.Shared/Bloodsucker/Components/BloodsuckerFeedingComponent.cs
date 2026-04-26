using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Added to a bloodsucker while they are actively feeding.
/// </summary>
[RegisterComponent]
public sealed partial class BloodsuckerFeedingComponent : Component
{
    [DataField]
    public EntityUid Target = EntityUid.Invalid;

    /// <summary>
    /// False while the vampire is in the aggressive-grab.
    /// Used to decide whether to show the interrupt message when the feed breaks.
    /// </summary>
    [DataField]
    public bool Silent = true;

    /// <summary>
    /// Whether the initial bite message has been shown.
    /// </summary>
    [DataField]
    public bool HasBitten = false;

    /// <summary>
    /// The victim's blood fraction.
    /// </summary>
    public float LastWarnedBloodFraction = 1f;
}
