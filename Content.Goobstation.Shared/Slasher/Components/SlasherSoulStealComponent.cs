using Content.Shared.Alert;
using Content.Shared.Damage;
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
    [ViewVariables]
    public EntityUid? ActionEntity;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherSoulSteal";

    [DataField]
    public ProtoId<AlertPrototype> Alert = "SlasherSoulBonus";

    [DataField]
    public DamageSpecifier AliveDamageBonusPerSoul = new()
    {
        DamageDict = new() { { "Slash", 1 } },
    };

    [DataField]
    public DamageSpecifier DeadDamageBonusPerSoul = new()
    {
        DamageDict = new() { { "Slash", 0.5 } },
    };

    /// <summary>
    /// Armor (percent damage reduction) granted per alive soul.
    /// </summary>
    [DataField]
    public float AliveArmorPercentPerSoul = 5f;

    /// <summary>
    /// Armor (percent damage reduction) granted per dead soul.
    /// </summary>
    [DataField]
    public float DeadArmorPercentPerSoul = 2.5f;

    /// <summary>
    /// Maximum armor, in percent damage reduction.
    /// </summary>
    [DataField]
    public float ArmorCap = 70f;

    /// <summary>
    /// Current total armor, in percent damage reduction (0-100).
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float ArmorPercent;

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
    /// The sound to play when reaching ascendance.
    /// </summary>
    [DataField]
    public SoundSpecifier AscendanceSound =
               new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/SlasherAscendance.ogg")
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
    [ViewVariables]
    public bool HasAscended;

    /// <summary>
    /// Optional starting gear to equip when this slasher ascends (set by kit selection).
    /// </summary>
    [ViewVariables]
    public ProtoId<StartingGearPrototype>? AscensionGear;

    [ViewVariables]
    public string AscendanceAnnouncementKey = "slasher-soulsteal-ascendance";

    /// <summary>
    /// Number of total souls required to unlock possession ability.
    /// </summary>
    [DataField]
    public int PossessionSoulThreshold = 10;

    /// <summary>
    /// Whether the possession ability has been unlocked.
    /// </summary>
    [ViewVariables]
    public bool HasUnlockedPossession;

    /// <summary>
    /// Amount of ammonia gas moles to release on successful soul steal.
    /// </summary>
    [DataField]
    public float MolesAmmonia = 700f;

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
    /// Cached applied damage bonus so we can reapply if machete is resummoned.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public DamageSpecifier TotalAppliedDamageBonus = new();

    /// <summary>
    /// Last known machete entity to which we applied damage components.
    /// </summary>
    [ViewVariables]
    public EntityUid? LastMachete;

    /// <summary>
    /// How often lights flicker around the ascended slasher.
    /// </summary>
    [DataField]
    public TimeSpan LightFlickerInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The next time lights should flicker.
    /// </summary>
    [ViewVariables]
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
