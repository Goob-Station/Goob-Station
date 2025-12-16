using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Atmos;
using Robust.Shared.Prototypes;
using Content.Shared.Materials;
using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/nuclearreactor.dm

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NuclearReactorComponent : Component
{
    /// <summary>
    /// Width of the reactor grid
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public int ReactorGridWidth = 7;

    /// <summary>
    /// Height of the reactor grid
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public int ReactorGridHeight = 7;

    public readonly int ReactorOverheatTemp = 1200;
    public readonly int ReactorFireTemp = 1500;
    public readonly int ReactorMeltdownTemp = 2000;

    // Making this a DataField causes the game to explode, neat
    /// <summary>
    /// 2D grid of reactor components, or null where there are no components. Size is ReactorGridWidth x ReactorGridHeight
    /// </summary>
    public ReactorPartComponent?[,] ComponentGrid;

    /// <summary>
    /// Dictionary of data that determines the reactor grid's visuals
    /// </summary>
    [AutoNetworkedField]
    public Dictionary<Vector2i, ReactorCapVisualData> VisualData = [];

    // Woe, 3 dimensions be upon ye
    /// <summary>
    /// 2D grid of lists of neutrons in each grid slot of the component grid
    /// </summary>
    public List<ReactorNeutron>[,] FluxGrid;

    /// <summary>
    /// Number of neutrons that hit the edge of the reactor grid last tick
    /// </summary>
    [ViewVariables]
    public float RadiationLevel = 0;

    /// <summary>
    /// Gas mixtrue currently in the reactor
    /// </summary>
    public GasMixture? AirContents;

    /// <summary>
    /// Reactor casing temperature
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Temperature = Atmospherics.T20C;

    /// <summary>
    /// Thermal mass. Basically how much energy it takes to heat this up 1Kelvin
    /// </summary>
    [DataField]
    public float ThermalMass = 420 * 2000; // specific heat capacity of steel (420 J/KgK) * mass of reactor (Kg)

    /// <summary>
    /// Volume of gas to process each tick
    /// </summary>
    [DataField]
    public float ReactorVesselGasVolume = 200;

    /// <summary>
    /// Flag indicating the reactor is overheating
    /// </summary>
    [ViewVariables]
    public bool IsSmoking = false;

    /// <summary>
    /// Flag indicating the reactor is on fire
    /// </summary>
    [ViewVariables]
    public bool IsBurning = false;

    /// <summary>
    /// Flag indicating total meltdown has happened
    /// </summary>
    [DataField, ViewVariables]
    public bool Melted = false;

    /// <summary>
    /// The set insertion level of the control rods
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float ControlRodInsertion = 2;

    /// <summary>
    /// The actual insertion level of the control rods
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float AvgInsertion = 0;

    /// <summary>
    /// Sound that plays globally on meltdown
    /// </summary>
    public SoundSpecifier MeltdownSound = new SoundPathSpecifier("/Audio/_FarHorizons/Machines/meltdown_siren.ogg");

    /// <summary>
    /// Radio channel to send alerts to
    /// </summary>
    [DataField]
    public string EngineeringChannel = "Engineering";

    /// <summary>
    /// Last reported temperature during overheat events
    /// </summary>
    [ViewVariables]
    public float LastSendTemperature = Atmospherics.T20C;

    /// <summary>
    /// If the reactor has given the nuclear emergency warning
    /// </summary>
    [ViewVariables]
    public bool HasSentWarning = false;

    /// <summary>
    /// Alert level to set after meltdown
    /// </summary>
    [DataField]
    public string MeltdownAlertLevel = "yellow";

    /// <summary>
    /// The minimum radiation from the melted reactor
    /// </summary>
    [DataField]
    public float MeltdownRadiation = 10;

    /// <summary>
    /// How quickly radiation decreases
    /// </summary>
    /// <remarks>Cannot be less than 1</remarks>
    [DataField]
    public float RadiationStability = 2;

    /// <summary>
    /// The maximum radiation the reactor can emit during normal operation
    /// </summary>
    [DataField]
    public float MaximumRadiation = 50;

    /// <summary>
    /// The maximum thermal power the reactor is expected to produce
    /// </summary>
    /// <remarks>This will NOT stop the reactor from making more than this value</remarks>
    [DataField]
    public float MaximumThermalPower = 10000000;

    /// <summary>
    /// The estimated thermal power the reactor is making
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float ThermalPower = 0;
    public int ThermalPowerCount = 0;
    public int ThermalPowerPrecision = 128;

    [ViewVariables]
    public EntityUid? AlarmAudioHighThermal;
    [ViewVariables]
    public EntityUid? AlarmAudioHighTemp;
    [ViewVariables]
    public EntityUid? AlarmAudioHighRads;

    [ViewVariables]
    public ItemSlot PartSlot = new();

    /// <summary>
    /// Grid of temperature values
    /// </summary>
    public double[,] TemperatureGrid;

    /// <summary>
    /// Grid of neutron counts
    /// </summary>
    public int[,] NeutronGrid;

    /// <summary>
    /// The selected prefab
    /// </summary>
    [DataField]
    public string Prefab = "normal";

    /// <summary>
    /// Flag indicating the reactor should apply the selected prefab
    /// </summary>
    [DataField]
    public bool ApplyPrefab = false;

    /// <summary>
    /// Material the reactor is made out of
    /// </summary>
    [DataField("material")]
    public ProtoId<MaterialPrototype> Material = "Steel";

    /// <summary>
    /// Determines the spacing and position of the visual grid. Measured in pixels.
    /// </summary>
    /// <remarks>
    /// [0] Spacing along the x axis<br/>
    /// [1] Spacing along the y axis<br/>
    /// [2] Offset of the center along the x axis<br/>
    /// [3] Offset of the center along the y axis
    /// </remarks>
    [DataField]
    public int[] Gridbounds = [ 18, 15, 0, 5 ];

    #region Pipe Connections
    /// <summary>
    /// Name of the pipe node
    /// </summary>
    [DataField]
    public string PipeName { get; set; } = "pipe";

    /// <summary>
    /// Inlet entity
    /// </summary>
    [ViewVariables]
    public EntityUid? InletEnt;

    /// <summary>
    /// Position of the inlet entity
    /// </summary>
    [DataField]
    public Vector2 InletPos = new(-2, -1);

    /// <summary>
    /// Rotation of the inlet entity, in degrees
    /// </summary>
    [DataField]
    public float InletRot = -90;

    /// <summary>
    /// Outlet entity
    /// </summary>
    [ViewVariables]
    public EntityUid? OutletEnt;

    /// <summary>
    /// Position of the outlet entity
    /// </summary>
    [DataField]
    public Vector2 OutletPos = new(2, 1);

    /// <summary>
    /// Rotation of the outlet entity, in degrees
    /// </summary>
    [DataField]
    public float OutletRot = 90;

    /// <summary>
    /// Name of the prototype of the arrows that indicate flow on inspect
    /// </summary>
    [DataField]
    public EntProtoId ArrowPrototype = "ReactorFlowArrow";

    #endregion

    #region Device Network
    /// <summary>
    /// The proto ID of the "Retract Control Rods" sink port
    /// </summary>
    [DataField("controlRodRetractPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string ControlRodRetractPort = "RetractControlRods";

    /// <summary>
    /// The proto ID of the "Insert Control Rods" sink port
    /// </summary>
    [DataField("controlRodInsertPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string ControlRodInsertPort = "InsertControlRods";

    /// <summary>
    /// The signal state of the retract control rods port
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public SignalState RetractPortState = SignalState.Low;

    /// <summary>
    /// The signal state of the insert control rods port
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public SignalState InsertPortState = SignalState.Low;
    #endregion

    #region Debug
    [ViewVariables(VVAccess.ReadOnly)]
    public int NeutronCount = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public int MeltedParts = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public int DetectedControlRods = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public float TotalNRads = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public float TotalRads = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public float TotalSpent = 0;
    #endregion
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class ReactorCapVisualData
{
    public Color color = Color.Black;
    public string cap = "";
}