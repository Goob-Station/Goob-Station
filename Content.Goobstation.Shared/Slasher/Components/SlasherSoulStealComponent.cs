using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the Soul Steal action and tracks cumulative bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherSoulStealComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEntity;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherSoulSteal";

    /// <summary>
    /// Flat brute bonus per alive soul stolen (applied to machete melee and throw).
    /// </summary>
    [DataField]
    public float AliveBruteBonusPerSoul = 2.5f;

    /// <summary>
    /// Flat brute bonus per corpse soul stolen (reduced).
    /// </summary>
    [DataField]
    public float DeadBruteBonusPerSoul = 1.5f;

    /// <summary>
    /// Max health bonus per alive soul.
    /// </summary>
    [DataField]
    public int AliveHealthBonusPerSoul = 25;

    /// <summary>
    /// Max health bonus per dead soul.
    /// </summary>
    [DataField]
    public int DeadHealthBonusPerSoul = 10;

    /// <summary>
    /// How long it takes to perform soul steal.
    /// </summary>
    [DataField]
    public int Soulstealdoafterduration = 15;

    /// <summary>
    /// The sound to play when soul steal completes.
    /// </summary>
    [DataField]
    public SoundSpecifier SoulStealSound =
               new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/SlasherSoulSteal.ogg")
               {
                   Params = AudioParams.Default
                       .WithVolume(-8f)
                       .WithMaxDistance(10f)
               };

    /// <summary>
    /// Total alive souls stolen.
    /// </summary>
    [ViewVariables]
    public int AliveSouls;

    /// <summary>
    /// Total dead souls stolen.
    /// </summary>
    [ViewVariables]
    public int DeadSouls;

    /// <summary>
    /// Cached applied brute bonus so we can reapply if machete is resummoned.
    /// </summary>
    [ViewVariables]
    public float TotalAppliedBruteBonus;

    /// <summary>
    /// Cached applied health bonus so we avoid double stacking.
    /// </summary>
    [ViewVariables]
    public int TotalAppliedHealthBonus;

    /// <summary>
    /// Last known machete entity to which we applied damage components.
    /// </summary>
    [ViewVariables]
    public EntityUid? LastMachete;


}

/// <summary>
/// Soul steal targeted action event.
/// </summary>
public sealed partial class SlasherSoulStealEvent : EntityTargetActionEvent
{
}

/// <summary>
/// Raised to amputate all limbs (arms/legs/hands/feet) from a target, sparing head and torso.
/// </summary>
/* Unused for now
public sealed partial class SlasherSoulStealAmputateEvent : EntityEventArgs
{
    public EntityUid Target { get; }
    public EntityUid? User { get; }

    public SlasherSoulStealAmputateEvent(EntityUid target, EntityUid? user = null)
    {
        Target = target;
        User = user;
    }
}
*/
