using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]

/// <summary>
/// Keeps this entity tethered to nearby anchor points. If it wanders too far from all anchors,
/// it starts taking periodic penalty ticks. Stray for long enough and the "leash" breaks entirely.
/// </summary>
public sealed partial class ProximityLeashComponent : Component
{
    // Translator note: ticks are just the time in which you've been in the red aka too far away from the anchor.

    /// <summary>
    /// Tracks time since the last tick.
    /// </summary>
    public float DamageAccumulator = 0f;

    /// <summary>
    /// How many ticks have fired since this entity went out of range.
    /// Resets to zero when it returns to range.
    /// </summary>
    public int TickCounter = 0;

    /// <summary>
    /// How far this entity can roam from the nearest anchor before ticks start.
    /// </summary>
    [DataField]
    public float MaxDistance = 20f;

    /// <summary>
    /// How often a tick fires while out of range.
    /// </summary>
    [DataField]
    public TimeSpan TickInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How many ticks before the leash is considered broken and consequences are applied.
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
