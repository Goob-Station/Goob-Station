namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(PowerMetricSystem))]
public sealed partial class PowerMetricComponent : Component
{
    [DataField]
    public double UnpoweredCost = 1000.0;

    [DataField]
    public TimeSpan BaselineDelay = TimeSpan.FromMinutes(3);

    [ViewVariables]
    public HashSet<EntityUid> BaselineUnpowered = new();

    [ViewVariables]
    public bool BaselineCaptured;
}
