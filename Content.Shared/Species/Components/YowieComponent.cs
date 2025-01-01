using Robust.Shared.GameStates;

namespace Content.Shared.Species.Components;
/// <summary>
/// This will apply a movespeed multiplier on an entity when outerlayer item is worn
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class YowieComponent : Component
{
    /// <summary>
    /// Movement speed multiplier, applied when worn
    /// </summary>
    [DataField(required: true)]
    public float SoftSuitSpeedMultiplier = default!;

    /// <summary>
    /// Current state of outerlayer inventory slot
    /// </summary>
    [DataField]
    public bool OuterLayerEquipped = false;
}
