using Content.Shared.Atmos;
using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;

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
    /// Current RPM of turbine
    /// </summary>
    [ViewVariables, AutoNetworkedField]
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
    [ViewVariables]
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

    [ViewVariables]
    public EntityUid? AlarmAudioOvertemp;
    [ViewVariables]
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

    [DataField]
    public string PipeName { get; set; } = "pipe";
    [ViewVariables]
    public EntityUid? InletEnt;
    [ViewVariables]
    public EntityUid? OutletEnt;

    #region Debug
    [ViewVariables(VVAccess.ReadOnly)]
    public bool HasPipes = false;
    [ViewVariables(VVAccess.ReadOnly)]
    public float SupplierMaxSupply = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    public float LastVolumeTransfer = 0;
    #endregion
}
