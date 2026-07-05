namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Stamped on clothing when a washing machine finishes its cycle. Equipping the item to the
/// inner slot before <see cref="Expiry"/> grants a timed fresh-laundry mood buff (TauCeti-style).
/// Only ever read server-side (mood), so it is not networked.
/// </summary>
[RegisterComponent]
public sealed partial class FreshLaundryComponent : Component
{
    /// <summary>Time after which wearing the item no longer grants the buff.</summary>
    [DataField]
    public TimeSpan Expiry;
}
