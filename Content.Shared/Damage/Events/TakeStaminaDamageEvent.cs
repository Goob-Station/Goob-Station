using Content.Shared.Damage.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Damage.Events;

/// <summary>
/// The entity is going to be hit,
/// give opportunities to change the damage or other stuff.
/// </summary>
// goobstation - stun resistance. try not to modify this event allat much
public sealed class TakeStaminaDamageEvent : HandledEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;

    public Entity<StaminaComponent>? Target;

    /// <summary>
    /// The multiplier. Generally, try to use *= or /= instead of overwriting.
    /// </summary>
    public float Multiplier = 1;

    /// <summary>
    /// The flat modifier. Generally, try to use += or -= instead of overwriting.
    /// </summary>
    public float FlatModifier = 0;

    public TakeStaminaDamageEvent(Entity<StaminaComponent> target)
    {
        Target = target;
    }
}
