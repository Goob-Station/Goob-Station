using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.Tools;
using Content.Shared.Atmos;
using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System.Numerics;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/turbine.dm

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TurbineComponent : Component
{
    /// <summary>
    /// Power generated last tick
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float LastGen = 0;

    /// <summary>
    /// Watts per revolution
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StatorLoad = 35000;

    /// <summary>
    /// Maximum setting of stator load
    /// </summary>
    [DataField]
    public float StatorLoadMax = 500000;

    /// <summary>
    /// Current RPM of turbine
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float RPM = 0;

    /// <summary>
    /// Turbine's resistance to change in RPM
    /// </summary>
    [DataField]
    public float TurbineMass = 1000;

    /// <summary>
    /// Most efficient power generation at this value, overspeed at 1.2*this
    /// </summary>
    [DataField]
    public float BestRPM = 600;

    /// <summary>
    /// RPM the animation is playing at
    /// </summary>
    [ViewVariables]
    public float AnimRPM = 0;

    /// <summary>
    /// Volume of gas to process per tick for power generation
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FlowRate = Atmospherics.MaxTransferRate;

    /// <summary>
    /// Maximum volume of gas to process per tick
    /// </summary>
    [DataField]
    public float FlowRateMax = Atmospherics.MaxTransferRate * 5;

    [DataField]
    public float OutputPressure = Atmospherics.MaxOutputPressure * 3;

    /// <summary>
    /// Max/min temperatures
    /// </summary>
    [DataField]
    public float MaxTemp = 3000;
    [DataField]
    public float MinTemp = Atmospherics.T20C;

    /// <summary>
    /// Health of the turbine
    /// </summary>
    [DataField, AutoNetworkedField]
    public int BladeHealth = 15;

    /// <summary>
    /// Maximum health of the turbine
    /// </summary>
    [DataField, AutoNetworkedField]
    public int BladeHealthMax = 15;

    /// <summary>
    /// If the turbine is functional or not
    /// </summary>
    [DataField, ViewVariables, AutoNetworkedField]
    public bool Ruined = false;

    /// <summary>
    /// Flag indicating the turbine is sparking
    /// </summary>
    [ViewVariables]
    public bool IsSparking = false;

    /// <summary>
    /// Flag indicating the turbine is smoking
    /// </summary>
    [ViewVariables]
    public bool IsSmoking = false;

    /// <summary>
    /// Flag for indicating that energy available is less than needed to turn the turbine
    /// </summary>
    [ViewVariables]
    public bool Stalling = false;

    /// <summary>
    /// Flag for RPM being > BestRPM*1.2
    /// </summary>
    [ViewVariables]
    public bool Overspeed = false;

    /// <summary>
    /// Flag for gas tempurature being > MaxTemp - 500
    /// </summary>
    [ViewVariables]
    public bool Overtemp = false;

    /// <summary>
    /// Flag for gas tempurature being < MinTemp
    /// </summary>
    [ViewVariables]
    public bool Undertemp = false;

    /// <summary>
    /// Adjustment for power generation
    /// </summary>
    [DataField]
    public float PowerMultiplier = 1;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? AlarmAudioOvertemp;
    [ViewVariables, AutoNetworkedField]
    public EntityUid? AlarmAudioUnderspeed;

    /// <summary>
    /// Length of repair do-after, in seconds
    /// </summary>
    [DataField]
    public float RepairDelay = 5;

    /// <summary>
    /// Amount of fuel consumed for repair
    /// </summary>
    [DataField]
    public float RepairFuelCost = 15;

    /// <summary>
    /// Tool capability needed to repair
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype> RepairTool = "Welding";

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
    public Vector2 InletPos = new(-1, -1);

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
    public Vector2 OutletPos = new(1, -1);

    /// <summary>
    /// Rotation of the outlet entity, in degrees
    /// </summary>
    [DataField]
    public float OutletRot = 90;

    /// <summary>
    /// Name of the prototype of the arrows that indicate flow on inspect
    /// </summary>
    [DataField]
    public EntProtoId ArrowPrototype = "TurbineFlowArrow";

    /// <summary>
    /// Name of the prototype of the pipes the turbine uses to connect to the pipe network
    /// </summary>
    [DataField]
    public EntProtoId PipePrototype = "TurbineGasPipe";
    #endregion

    #region Device Network
    /// <summary>
    /// The proto ID of the "Speed: High" source port
    /// </summary>
    [DataField("speedHighPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string SpeedHighPort = "TurbineSpeedHigh";

    /// <summary>
    /// The proto ID of the "Speed: Low" source port
    /// </summary>
    [DataField("speedLowPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string SpeedLowPort = "TurbineSpeedLow";

    /// <summary>
    /// The proto ID of the "Turbine Data" source port
    /// </summary>
    [DataField("turbineDataPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string TurbineDataPort = "GasTurbineDataSender";

    /// <summary>
    /// The proto ID of the "Increase Stator Load" sink port
    /// </summary>
    [DataField("statorLoadIncreasePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string StatorLoadIncreasePort = "IncreaseStatorLoad";

    /// <summary>
    /// The proto ID of the "Decrease Stator Load" sink port
    /// </summary>
    [DataField("statorLoadDecreasePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string StatorLoadDecreasePort = "DecreaseStatorLoad";

    /// <summary>
    /// The signal state of the increase stator load port
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public SignalState IncreasePortState = SignalState.Low;

    /// <summary>
    /// The signal state of the decrease stator load port
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public SignalState DecreasePortState = SignalState.Low;
    #endregion

    #region Debug
    [ViewVariables(VVAccess.ReadOnly)]
    public bool HasPipes = false;
    [ViewVariables(VVAccess.ReadOnly)]
    public float SupplierMaxSupply = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public float SupplierLastSupply = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public float LastVolumeTransfer = 0;
    #endregion
}
