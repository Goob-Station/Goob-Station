namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(AnomalyMetric))]
public sealed partial class AnomalyMetricComponent : Component
{
    /// <summary>
    ///   Cost of a growing anomaly
    /// </summary>
    [DataField("growingCost")]
    public float GrowingCost = 40.0f;

    /// <summary>
    ///   Cost of a dangerous anomaly
    /// </summary>
    [DataField("severityCost")]
    public float SeverityCost = 20.0f;

    /// <summary>
    ///   Cost of any anomaly
    /// </summary>
    [DataField("dangerCost")]
    public float BaseCost = 10.0f;
}
