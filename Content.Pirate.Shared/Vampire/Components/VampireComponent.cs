using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Pirate.Shared.Vampire.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]

public sealed partial class VampireComponent : Component
{
    /// <summary>
    /// Chosen vampire class prototype id, once selected.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? ChosenClassId;

    /// <summary>
    /// Default abilities, they will be added at start.
    /// </summary>
    [DataField]
    public List<EntProtoId> BaseVampireActions = new()
    {
        "ActionVampireGlare",
        "ActionVampireRejuvenateI",
        "ActionVampireSleep"
    };

    /// <summary>
    /// Core action ids that systems need to manage explicitly.
    /// </summary>
    [DataField]
    public EntProtoId ClassSelectActionId = "ActionClassSelectId";

    [DataField]
    public List<EntProtoId> RejuvenateActions = new()
    {
        "ActionVampireRejuvenateI",
        "ActionVampireRejuvenateII"
    };

    /// <summary>
    /// Lifetime total blood drunk. Used for unlocking abilities.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalBlood = 0;

    /// <summary>
    /// Total blood drunk by this vampire, used for blood cost calculations.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int DrunkBlood = 0;

    /// <summary>
    /// bites since last time blindness was applied.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int BlindInc = 0;

    /// <summary>
    /// Metabolizer types applied to the vampire's stomach so it processes blood like a trait vampire.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<MetabolizerTypePrototype>> MetabolizerPrototypes = new() { "Vampiric", "Animal" };

    /// <summary>
    /// damage per 1u of blood drawn from target
    /// </summary>
    [DataField]
    public float SipPierceDamage = 0.05f;
    /// <summary>
    /// how much blood drawn from target is actually drank vs spilled from humanoids
    /// </summary>
    [DataField]
    public float HumanoidEfficiency = 0.5f;
    /// <summary>
    /// how much blood drawn from target is actually drank vs spilled from animals
    /// </summary>
    [DataField]
    public float NonHumanoidEfficiency = 0.125f;
    /// <summary>
    /// how much blood is gained when the target is dead (0 disables drinking from the dead completely)
    /// </summary>
    [DataField]
    public float DeadEfficiency = 0.75f;
    /// <summary>
    /// How much blood is gained when the target has not yet rotted (less than 30 seconds since death)
    /// </summary>
    [DataField]
    public float Rot0Efficiency = 1.0f;
    /// <summary>
    /// How much blood is gained when the target is at the initial stage of rot (less than 3:30 since death)
    /// </summary>
    [DataField]
    public float Rot1Efficiency = 0.5f;
    /// <summary>
    /// How much blood is gained when the target is at the mid stage of rot (less than 6:45 since death)
    /// </summary>
    [DataField]
    public float Rot2Efficiency = 0.25f;
    /// <summary>
    /// How much blood is gained when the target is at the late stage of rot (less than 10:00 since death)
    /// </summary>
    [DataField]
    public float Rot3Efficiency = 0.1f;
    /// <summary>
    /// How much blood is gained when the target is fully rotted (more than 10:00 since death)
    /// </summary>
    [DataField]
    public float Rot4Efficiency = 0.0f;

    /// <summary>
    /// Current blood fullness used instead of normal food needs.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField, AutoNetworkedField]
    public float BloodFullness = 90f;

    /// <summary>
    /// Max amount of blood which can be drained from one person.
    /// </summary>
    [DataField]
    public float MaxBloodFullness = 200f;

    /// <summary>
    /// Decay rate per second for blood fullness.
    /// </summary>
    [DataField]
    public float FullnessDecayPerSecond = 0.15f;

    /// <summary>
    /// When <see cref="BloodFullness"/> is empty, apply a movement slowdown.
    /// </summary>
    [DataField]
    public float StarvationWalkSpeedModifier = 0.7f;
    [DataField]
    public float StarvationSprintSpeedModifier = 0.7f;

    /// <summary>
    /// When <see cref="BloodFullness"/> is empty, drain this much <see cref="DrunkBlood"/> per second.
    /// </summary>
    [DataField]
    public int StarvationDrunkBloodDrainPerSecond = 2;

    /// <summary>
    /// Action entities of the vampire, used as ActionId -> EntityUid.
    /// </summary>
    public Dictionary<EntProtoId, EntityUid> ActionEntities = new();

    /// <summary>
    /// tracking how much blood was drunk from each target.
    /// </summary>
    public Dictionary<EntityUid, int> BloodDrunkFromTargets = new();

    [DataField]
    public int MaxBloodPerTarget = 200;
    public EntityUid? SpawnedClaws = null;
    [DataField]
    public int ClassSelectThreshold = 150;
    [DataField]
    public int ActionRefreshThreshold = 5;

    [DataField]
    public TimeSpan HolyTickDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public float HolyPlaceRange = 8f;

    public bool HadWeakToHoly;
    public bool HadAlwaysTakeHoly;

    /// <summary>
    /// Healing factors
    /// </summary>
    [DataField]
    public int VampHealBurn = 2;
    [DataField]
    public int VampHealBrute = 2;
    [DataField]
    public int VampHealAsphyxiation = 10;
    [DataField]
    public int VampHealPois = 4;

    [DataField]
    public ProtoId<ReagentPrototype> HolyWaterReagentId = "Holywater";
    [AutoPausedField]
    public TimeSpan NextHolyWaterTick = TimeSpan.Zero;
    public TimeSpan NextHolyPlaceTick = TimeSpan.Zero;
    public TimeSpan NextHolyPlacePopup = TimeSpan.Zero;

    public float StarvationDrunkBloodDrainAccumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    public int LastRefreshedBloodLevel = -1;

    [ViewVariables(VVAccess.ReadOnly), DataField, AutoNetworkedField]
    public bool FullPower = false;
    /// <summary>Amount of blood that must be drank for the vampire to be considered for max level</summary>
    [DataField]
    public int FullPowerThreshold = 1000;
    /// <summary>Amount of Unique Victims for the vampire to be considered for max level</summary>
    [DataField]
    public int FullPowerUniqueHumanoids = 8;
    /// <summary>Amount of blood drank for the vampire to be considered mid-level</summary>
    [DataField]
    public int MidPowerThreshold = 200;
    /// <summary>Amount of blood drank for the vampire to be considered high-level</summary>
    [DataField]
    public int HighPowerThreshold = 600;
    /// <summary>number of Unique victims the vampire has drank from so far</summary>
    [ViewVariables(VVAccess.ReadOnly), DataField, AutoNetworkedField]
    public int UniqueHumanoidVictims = 0;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextUpdate;

    [AutoPausedField]
    public TimeSpan LastUpdate;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);
}

[RegisterComponent]
public sealed partial class ShadowSnareBlindMarkerComponent : Component { }

[RegisterComponent]
public sealed partial class ShadowSnareEnsnareComponent : Component
{
    [DataField]
    public EntityUid Victim;
}
