using Robust.Shared.GameStates;
using Content.Shared._EinsteinEngines.Silicon.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared._EinsteinEngines.Silicon.Components;

/// <summary>
///     Component for defining a mob as a robot.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SiliconComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public short ChargeState = 10;

    [ViewVariables(VVAccess.ReadOnly)]
    public float OverheatAccumulator = 0.0f;

    /// <summary>
    ///     The last time the Silicon was drained.
    ///     Used for NPC Silicons to avoid over updating.
    /// </summary>
    /// <remarks>
    ///     Time between drains can be specified in
    ///     <see cref="SimpleStationCcvars.SiliconNpc"/>
    /// </remarks>
    public TimeSpan LastDrainTime = TimeSpan.Zero;

    /// <summary>
    ///     The Silicon's battery slot, if it has one.
    /// </summary>

    /// <summary>
    ///     Is the Silicon currently dead?
    /// </summary>
    public bool Dead = false;

    // BatterySystem took issue with how this was used, so I'm coming back to it at a later date, when more foundational Silicon stuff is implemented.
    // /// <summary>
    // ///     The entity to get the battery from.
    // /// </summary>
    // public EntityUid BatteryOverride? = EntityUid.Invalid;


    /// <summary>
    ///     The type of silicon this is.
    /// </summary>
    /// <remarks>
    ///     Any new types of Silicons should be added to the enum.
    ///     Setting this to Npc will delay charge state updates by LastDrainTime and skip battery heat calculations
    /// </remarks>
    [DataField(customTypeSerializer: typeof(EnumSerializer))]
    public Enum EntityType = SiliconType.Npc;

    /// <summary>
    ///     Is this silicon battery powered?
    /// </summary>
    /// <remarks>
    ///     If true, should go along with a battery component. One will not be added automatically.
    /// </remarks>
    [DataField]
    public bool BatteryPowered = false;

    /// <summary>
    ///     How much power is drained by this Silicon every second by default.
    /// </summary>
    [DataField]
    public float DrainPerSecond = 50f;


    /// <summary>
    ///     The percentages at which the silicon will enter each state.
    /// </summary>
    /// <remarks>
    ///     The Silicon will always be Full at 100%.
    ///     Setting a value to null will disable that state.
    ///     Setting Critical to 0 will cause the Silicon to never enter the dead state.
    /// </remarks>
    [DataField]
    public float? ChargeThresholdMid = 0.5f;

    /// <inheritdoc cref="ChargeThresholdMid"/>
    [DataField]
    public float? ChargeThresholdLow = 0.25f;

    /// <inheritdoc cref="ChargeThresholdMid"/>
    [DataField]
    public float? ChargeThresholdCritical = 0.1f;

    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";


    /// <summary>
    ///     The amount the Silicon will be slowed at each charge state.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<int, float> SpeedModifierThresholds = default!;

    [DataField]
    public float FireStackMultiplier = 1f;

    /// <summary>
    ///     Whether or not a Silicon will cancel all sleep events.
    ///     Maybe you want an android that can sleep as well as drink APCs? I'm not going to judge.
    /// </summary>
    [DataField]
    public bool DoSiliconsDreamOfElectricSheep;
}
