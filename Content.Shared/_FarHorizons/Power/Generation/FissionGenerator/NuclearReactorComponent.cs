using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Atmos;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

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
    [DataField]
    public float RadiationLevel = 0;

    /// <summary>
    /// Gas mixtrue currently in the reactor
    /// </summary>
    public GasMixture? AirContents;

    /// <summary>
    /// Reactor casing temperature
    /// </summary>
    [DataField]
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
    [DataField]
    public bool IsSmoking = false;

    /// <summary>
    /// Flag indicating the reactor is on fire
    /// </summary>
    [DataField]
    public bool IsBurning = false;

    /// <summary>
    /// Flag indicating total meltdown has happened
    /// </summary>
    [DataField]
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
    [DataField]
    public float AvgInsertion = 0;

    /// <summary>
    /// Sound that plays globally on meltdown
    /// </summary>
    public SoundSpecifier MeltdownSound = new SoundPathSpecifier("/Audio/_FarHorizons/Machines/meltdown_siren.ogg");

    /// <summary>
    /// Radio channel to send alerts to
    /// </summary>
    [DataField]
    public string AlertChannel = "Engineering";

    /// <summary>
    /// Alert level to set after meltdown
    /// </summary>
    [DataField]
    public string MeltdownAlertLevel = "yellow";

    /// <summary>
    /// The estimated thermal power the reactor is making
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public float ThermalPower = 0;
    public float[] ThermalPowerL1 = new float[32];
    public float[] ThermalPowerL2 = new float[32];

    public EntityUid? AlarmAudioHighThermal;
    public EntityUid? AlarmAudioHighTemp;
    public EntityUid? AlarmAudioHighRads;

    [DataField]
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
    [DataField]
    public bool ApplyPrefab = true;

    [DataField("inlet")]
    public string InletName { get; set; } = "inlet";

    [DataField("outlet")]
    public string OutletName { get; set; } = "outlet";

    #region Debug
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("neutrons")]
    public int NeutronCount = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("meltedParts")]
    public int MeltedParts = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("controlRods")]
    public int DetectedControlRods = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("totalN-Rads")]
    public float TotalNRads = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("totalRads")]
    public float TotalRads = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("spentFuel")]
    public float TotalSpent = 0;
    #endregion
}