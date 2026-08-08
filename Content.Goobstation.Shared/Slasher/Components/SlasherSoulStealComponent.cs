using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the Soul Steal action and tracks cumulative bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherSoulStealComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherSoulSteal";

    /// <summary>
    /// Flat slash bonus per alive soul stolen (applied to machete melee and throw).
    /// </summary>
    [DataField]
    public float AliveBruteBonusPerSoul = 1.5f;

    /// <summary>
    /// Flat slash bonus per corpse soul stolen.
    /// </summary>
    [DataField]
    public float DeadBruteBonusPerSoul = 0.75f;

    /// <summary>
    /// Armor (damage reduction) granted per alive soul.
    /// </summary>
    [DataField]
    public float AliveArmorPercentPerSoul = 0.05f;

    /// <summary>
    /// Armor (damage reduction) granted per dead soul.
    /// </summary>
    [DataField]
    public float DeadArmorPercentPerSoul = 0.025f;

    /// <summary>
    /// Maximum armor (damage reduction) reduction.
    /// </summary>
    [DataField]
    public float ArmorCap = 0.70f;

    /// <summary>
    /// Current total armor reduction (0-1).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ArmorReduction;

    /// <summary>
    /// Makes it so the target needs to have a missing limb.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RequireLimbLoss = true;

    /// <summary>
    /// How long it takes to perform soul steal.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Soulstealdoafterduration = 15;

    /// <summary>
    /// The sound to play when soul steal completes.
    /// </summary>
    [DataField]
    public SoundSpecifier SoulStealSound =
               new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/SlasherSoulSteal.ogg")
               {
                   Params = AudioParams.Default
                       .WithMaxDistance(10f)
               };

    /// <summary>
    /// The sound to play when reaching ascendance.
    /// </summary>
    [DataField]
    public SoundSpecifier AscendanceSound =
               new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/SlasherAscendance.ogg")
               {
                   Params = AudioParams.Default
                       .WithVolume(-7f)
               };

    /// <summary>
    /// Number of total souls required to trigger ascendance.
    /// </summary>
    [DataField]
    public int AscendanceSoulThreshold = 15;

    /// <summary>
    /// Whether the ascendance event has been triggered.
    /// </summary>
    [DataField]
    public bool HasAscended;

    /// <summary>
    /// Optional starting gear to equip when this slasher ascends (set by kit selection).
    /// </summary>
    [DataField]
    public ProtoId<StartingGearPrototype>? AscensionGear;

    [DataField]
    public string AscendanceAnnouncementKey = "slasher-soulsteal-ascendance";

    /// <summary>
    /// Prestige token recorded for the player when this slasher ascends (set by kit selection).
    /// If null, ascending grants no prestige unlock.
    /// </summary>
    [DataField]
    public string? AscensionId;

    /// <summary>
    /// Number of total souls required to unlock possession ability.
    /// </summary>
    [DataField]
    public int PossessionSoulThreshold = 10;

    /// <summary>
    /// Whether the possession ability has been unlocked.
    /// </summary>
    [DataField]
    public bool HasUnlockedPossession;

    /// <summary>
    /// Amount of ammonia gas moles to release on successful soul steal.
    /// </summary>
    [DataField]
    public float MolesAmmonia = 700f;

    /// <summary>
    /// Total alive souls stolen.
    /// </summary>
    [DataField]
    public int AliveSouls;

    /// <summary>
    /// Total dead souls stolen.
    /// </summary>
    [DataField]
    public int DeadSouls;

    /// <summary>
    /// Cached applied brute bonus so we can reapply if machete is resummoned.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TotalAppliedBruteBonus;

    /// <summary>
    /// Last known machete entity to which we applied damage components.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LastMachete;

    /// <summary>
    /// How often lights flicker around the ascended slasher.
    /// </summary>
    [DataField]
    public TimeSpan LightFlickerInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The next time lights should flicker.
    /// </summary>
    [DataField]
    public TimeSpan NextLightFlicker = TimeSpan.Zero;

    /// <summary>
    /// Radius around the ascended slasher in which lights will flicker.
    /// </summary>
    [DataField]
    public float LightFlickerRadius = 5f;

    /// <summary>
    /// Maximum number of lights to flicker per interval.
    /// </summary>
    [DataField]
    public int MaxLightsToFlicker = 3;
}
