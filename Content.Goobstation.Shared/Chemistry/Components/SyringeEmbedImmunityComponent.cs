using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components;

/// <summary>
/// Prevents syringes from injecting chemicals into embedded entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SyringeEmbedImmunityComponent : Component
{
    /// <summary>
    /// If true, the immunity can be bypassed by syringes with PierceArmorOverride set (e.g., CMO's rapid syringe gun).
    /// If false, the immunity is absolute and cannot be bypassed.
    /// False for things like Space Dragons.
    /// </summary>
    [DataField]
    public bool IsPenetrable = false;
}
