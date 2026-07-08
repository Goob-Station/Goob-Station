namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Marks clothing eligible for the fresh-laundry mood buff.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class FreshLaundryComponent : Component
{
    /// <summary>When the fresh-laundry buff expires.</summary>
    [DataField, AutoPausedField]
    public TimeSpan Expiry;
}
