using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the haste action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerHasteComponent : Component
{
    /// <summary>
    /// Speed at which the vampire is thrown toward the destination.
    /// </summary>
    [DataField]
    public float DashSpeed = 25f;

    /// <summary>
    /// Base knockdown on entities hit during the dash, in seconds.
    /// Scales with level: base + level * KnockdownPerLevel.
    /// </summary>
    [DataField]
    public float KnockdownBase = 1f;

    [DataField]
    public float KnockdownPerLevel = 0.5f;

    [DataField]
    public SoundSpecifier? DashSound = new SoundPathSpecifier("/Audio/Weapons/punchmiss.ogg");

    [DataField]
    public SoundSpecifier? HitSound = new SoundPathSpecifier("/Audio/Weapons/punch1.ogg");

    /// <summary>True while the vampire is mid-dash, gates collision handling.</summary>
    [DataField]
    public bool IsDashing;

    /// <summary>Entities already hit this dash, so we don't double-apply.</summary>
    [DataField]
    public HashSet<EntityUid> AlreadyHit = new();

    #region Generic

    /// <summary>
    /// The current level of this action.
    /// </summary>
    public int ActionLevel = 1;

    /// <summary>
    /// The highest level this action can become.
    /// </summary>
    public int MaxLevel = 5;

    /// <summary>
    /// Blood cost deducted from the vampire's bloodstream to activate.
    /// </summary>
    [DataField]
    public float BloodCost = 0f;

    /// <summary>
    /// Humanity lost when activating this action.
    /// </summary>
    [DataField]
    public float HumanityCost = 0f;

    /// <summary>
    /// If true, this action cannot be started while the vampire is in frenzy.
    /// </summary>
    [DataField]
    public bool DisabledInFrenzy = false;

    #endregion
}
