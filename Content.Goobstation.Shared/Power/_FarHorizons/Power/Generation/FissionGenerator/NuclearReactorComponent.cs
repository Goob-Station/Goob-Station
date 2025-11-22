using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Materials;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;

[RegisterComponent, NetworkedComponent]
public sealed partial class NuclearReactorComponent : Component
{
    public static int ReactorGridWidth = 7;
    public static int ReactorGridHeight = 7;
    public readonly int ReactorOverheatTemp = 1200;
    public readonly int ReactorFireTemp = 1500;
    public readonly int ReactorMeltdownTemp = 2000;

    // Making this a DataField causes the game to explode, neat
    /// <summary>
    /// 2D grid of reactor components, or null where there are no components. Size is ReactorGridWidth x ReactorGridHeight
    /// </summary>
    public ReactorPartComponent?[,] ComponentGrid = new ReactorPartComponent[ReactorGridWidth, ReactorGridHeight];

    // Woe, 3 dimensions be upon ye
    /// <summary>
    /// 2D grid of lists of neutrons in each grid slot of the component grid.
    /// </summary>
    public List<ReactorNeutron>[,] FluxGrid = new List<ReactorNeutron>[ReactorGridWidth, ReactorGridHeight];

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
    [ViewVariables]
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
    [ViewVariables]
    public bool Melted = false;

    /// <summary>
    /// The set insertion level of the control rods
    /// </summary>
    [DataField]
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
    public double[,] TemperatureGrid = new double[ReactorGridWidth, ReactorGridHeight];

    /// <summary>
    /// Grid of neutron counts
    /// </summary>
    public int[,] NeutronGrid = new int[ReactorGridWidth, ReactorGridHeight];

    /// <summary>
    /// Grid of entities that make up the visual reactor grid
    /// </summary>
    public NetEntity[,] VisualGrid = new NetEntity[ReactorGridWidth, ReactorGridHeight];

    /// <summary>
    /// The selected prefab
    /// </summary>
    [DataField]
    public string Prefab = "normal";

    /// <summary>
    /// Flag indicating the reactor should apply the selected prefab
    /// </summary>
    [ViewVariables]
    public bool ApplyPrefab = true;

    /// <summary>
    /// Material the reactor is made out of
    /// </summary>
    [DataField("material")]
    public ProtoId<MaterialPrototype> Material = "Steel";

    [DataField]
    public string PipeName { get; set; } = "pipe";
    [ViewVariables]
    public EntityUid? InletEnt;
    [ViewVariables]
    public EntityUid? OutletEnt;

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