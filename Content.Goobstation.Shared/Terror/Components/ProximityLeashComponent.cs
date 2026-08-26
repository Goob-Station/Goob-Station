using Content.Shared.Body.Systems;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Forces an entity to stay within X range of another entity.
/// After X amount of time outside X range of the anchor entity, pops up a warning.
/// Stay out long enough and it gibs the entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProximityLeashComponent : Component
{
    // Translator note: ticks are just the time in which you've been in the red aka too far away from the anchor.

    public TimeSpan NextTick;

    /// <summary>
    /// How many ticks since this entity went out of anchor range.
    /// Resets when it returns to range.
    /// </summary>
    public int TickCounter;

    /// <summary>
    /// How far this entity can go before ticks start counting.
    /// </summary>
    [DataField]
    public float MaxDistance = 20f;

    /// <summary>
    /// How often a tick fires while out of range.
    /// </summary>
    [DataField]
    public TimeSpan TickInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How many ticks before the leash is considered broken and the entity gibs.
    /// Set to 0 or negative to never trigger a break. But why would you even do that lol
    /// </summary>
    [DataField]
    public int BreakThreshold = 15;

    /// <summary>
    /// Only anchors in the same group as this value will count.
    /// So multiple different anchors can exist. Don't forget to set it in YAML.
    /// </summary>
    [DataField]
    public string LeashGroup = "default";
}
