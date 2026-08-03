using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Clothing;

/// <summary>
/// Raises when entity get splashed by SpillableComponent in MeleeHitEvent.
/// </summary>
[ByRefEvent]
public record struct SpillableCheckClothingEvent(Solution? SolutionAmount, ReactionMethod ReactionMethod) : IInventoryRelayEvent
{
    /// <summary>
    /// The solution that about to be spilled on the entity.
    /// </summary>
    public Solution? SolutionAmount = SolutionAmount;

    /// <summary>
    /// The reaction method that is used
    /// </summary>
    public readonly ReactionMethod ReactionMethod = ReactionMethod;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
