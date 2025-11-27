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
    /// Flat slash bonus per alive soul stolen (applied to machete melee and throw).
    /// </summary>
    [DataField]
    public float AliveBruteBonusPerSoul = 2.5f;

    /// <summary>
    /// Flat slash bonus per corpse soul stolen.
    /// </summary>
    [DataField]
    public float DeadBruteBonusPerSoul = 1.5f;

    /// <summary>
    /// Armor (damage reduction) granted per alive soul.
    /// </summary>
    [DataField]
    public float AliveArmorPercentPerSoul = 0.07f;

    /// <summary>
    /// Armor (damage reduction) granted per dead soul.
    /// </summary>
    [DataField]
    public float DeadArmorPercentPerSoul = 0.03f;

    /// <summary>
    /// Maximum armor (damage reduction) reduction.
    /// </summary>
    [DataField]
    public float ArmorCap = 0.94f;

    /// <summary>
    /// Current total armor reduction (0-1).
    /// </summary>
    [ViewVariables]
    public float ArmorReduction;

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
                       .WithMaxDistance(10f)
               };

    /// <summary>
    /// Amount of ammonia gas moles to release on successful soul steal.
    /// </summary>
    [DataField]
    public float MolesAmmonia = 1000f;

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
    /// Last known machete entity to which we applied damage components.
    /// </summary>
    [ViewVariables]
    public EntityUid? LastMachete;
}
