namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(DoorMetricSystem))]
public sealed partial class DoorMetricComponent : Component
{
    /// <summary>
    ///   Cost of all doors emagged door
    /// </summary>
    [DataField("emagCost"), ViewVariables(VVAccess.ReadWrite)]
    public double EmagCost = 200.0f;

    /// <summary>
    ///   Cost of all doors with no power
    /// </summary>
    [DataField("powerCost"), ViewVariables(VVAccess.ReadWrite)]
    public double PowerCost = 100.0f;

    /// <summary>
    ///   Cost of all firedoors holding pressure
    /// </summary>
    [DataField("pressureCost"), ViewVariables(VVAccess.ReadWrite)]
    public double PressureCost = 200.0f;

    /// <summary>
    ///   Cost of all firedoors holding temperature
    /// </summary>
    [DataField("fireCost"), ViewVariables(VVAccess.ReadWrite)]
    public double FireCost = 400.0f;
}
