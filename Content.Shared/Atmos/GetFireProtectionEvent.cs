using Content.Shared.Inventory;

namespace Content.Shared.Atmos;

/// <summary>
/// Raised on a burning entity to check its fire protection.
/// Damage taken is multiplied by the final amount, but not temperature.
/// TemperatureProtection is needed for that.
/// </summary>
[ByRefEvent]
public sealed class GetFireProtectionEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;

    /// <summary>
    /// What to multiply the fire damage by.
    /// If this is 0 then it's ignored
    /// </summary>
    public float Multiplier;

    /// <summary>
    /// Goobstation - The entity the event was originally raised on.
    /// </summary>
    public readonly EntityUid Target;

    public GetFireProtectionEvent(EntityUid target) // Goobstation
    {
        Multiplier = 1f;
        Target = target; // Goobstation
    }

    /// <summary>
    /// Reduce fire damage taken by a percentage.
    /// </summary>
    public void Reduce(float by)
    {
        Multiplier -= by;
    }
}
