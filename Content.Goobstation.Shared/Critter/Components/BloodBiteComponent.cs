using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Critter.Components;

/// <summary>
/// Configuration for the Blood Bite action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodBiteComponent : Component
{
    /// <summary>
    /// Healing applied each successful drain tick.
    /// </summary>
    [DataField]
    public DamageSpecifier HealSpecifier = new();

    /// <summary>
    /// Blood drained from target per tick.
    /// </summary>
    [DataField]
    public float BloodDrainAmount = 2f;

    /// <summary>
    /// Multiplier applied to first bite bleed.
    /// </summary>
    [DataField]
    public float InitialBleedMultiplier = 0.5f;

    /// <summary>
    /// Delay before first bite completes.
    /// </summary>
    [DataField]
    public float StartDelay = 1f;

    /// <summary>
    /// Delay between drain ticks.
    /// </summary>
    [DataField]
    public float SipDelay = 1f;

    /// <summary>
    /// Max allowed range to continue feeding.
    /// </summary>
    [DataField]
    public float Range = 1.5f;

    /// <summary>
    /// Whether DoAfter breaks on movement.
    /// </summary>
    [DataField]
    public bool BreakOnMove = true;

    /// <summary>
    /// Whether DoAfter breaks on damage.
    /// </summary>
    [DataField]
    public bool BreakOnDamage = true;

    /// <summary>
    /// Whether the DoAfter is hidden.
    /// </summary>
    [DataField]
    public bool HiddenDoAfter = true;

    /// <summary>
    /// Whether drained blood is transferred to the attacker.
    /// </summary>
    [DataField]
    public bool TransferBlood = true;

    [DataField]
    public SoundSpecifier? DrinkSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");

    /// <summary>
    /// Self explanatory.
    /// </summary>
    public bool ShouldPlaySound;
}
