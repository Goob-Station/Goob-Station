using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.Tools;
using Content.Shared.Atmos;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TurbineComponent : Component
{
    /// <summary>
    /// Power generated last tick
    /// </summary>
    [DataField]
    public float LastGen = 0;

    /// <summary>
    /// Watts per revolution
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StatorLoad = 35000;

    /// <summary>
    /// Current RPM of turbine
    /// </summary>
    [DataField("RPM"), AutoNetworkedField]
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
    [DataField]
    public bool Ruined = false;

    /// <summary>
    /// Flag indicating the turbine is sparking
    /// </summary>
    [DataField]
    public bool IsSparking = false;

    /// <summary>
    /// Flag indicating the turbine is smoking
    /// </summary>
    [DataField]
    public bool IsSmoking = false;

    /// <summary>
    /// Flag for indicating that energy available is less than needed to turn the turbine
    /// </summary>
    [DataField]
    public bool Stalling = false;

    /// <summary>
    /// Flag for RPM being > BestRPM*1.2
    /// </summary>
    [DataField]
    public bool Overspeed = false;

    /// <summary>
    /// Flag for gas tempurature being > MaxTemp - 500
    /// </summary>
    [DataField]
    public bool Overtemp = false;

    /// <summary>
    /// Flag for gas tempurature being < MinTemp
    /// </summary>
    [DataField]
    public bool Undertemp = false;

    /// <summary>
    /// Adjustment for power generation
    /// </summary>
    [DataField]
    public float PowerMultiplier = 1;

    [DataField]
    public EntityUid? AlarmAudioOvertemp;

    [DataField]
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
    public ProtoId<ToolQualityPrototype> RepairTool = "Welding";

    [DataField("inlet")]
    public string InletName { get; set; } = "inlet";

    [DataField("outlet")]
    public string OutletName { get; set; } = "outlet";

    #region Debug
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("HasPipes")]
    public bool HasPipes = false;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("SupplierMaxSupply")]
    public float SupplierMaxSupply = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("LastVolumeTransfer")]
    public float LastVolumeTransfer = 0;
    #endregion
}
