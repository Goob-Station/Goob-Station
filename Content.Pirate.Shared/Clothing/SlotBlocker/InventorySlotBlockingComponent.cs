using Robust.Shared.GameStates;


namespace Content.Pirate.Shared.Clothing.SlotBlocker;


/// <summary>
///     Applied to entities with an inventory component. Allows clothing on this entity to block certain slots.
///     This component is not required when the clothing in question has a SlotBlockerComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InventorySlotBlockingComponent : Component
{
}
