using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Harvestable;

/// <summary>
/// Simple component for harvestables. "Click on me to get loot" behavior.
/// </summary>
[RegisterComponent]
public sealed partial class HarvestableComponent : Component
{
    // Harvest loot.
    [DataField(required: true)]
    public EntProtoId? Loot;

    // Harvest doAfter delay.
    [DataField]
    public float Delay = 1f;
}
