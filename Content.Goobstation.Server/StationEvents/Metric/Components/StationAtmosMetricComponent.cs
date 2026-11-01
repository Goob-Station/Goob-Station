using Content.Shared.Atmos;

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(StationAtmosMetricSystem))]
public sealed partial class StationAtmosMetricComponent : Component
{
    [DataField]
    public double SpacedTileCost = 0.5;

    [DataField]
    public double FireTileCost = 1.0;

    [DataField]
    public double ToxicTileCost = 0.25;

    [DataField]
    public TimeSpan BaselineDelay = TimeSpan.FromMinutes(2);

    [DataField]
    public float SpacedPressure = Atmospherics.HazardLowPressure;

    [DataField]
    public Dictionary<Gas, float> ToxicGasMoles = new()
    {
        { Gas.Plasma, 0.5f },
        { Gas.Tritium, 0.5f },
        { Gas.Frezon, 0.5f },
        { Gas.Ammonia, 1f },
        { Gas.NitrousOxide, 2f },
        { Gas.CarbonDioxide, 5f },
    };

    [ViewVariables]
    public HashSet<(EntityUid Grid, Vector2i Tile)> BaselineTiles = new();

    [ViewVariables]
    public bool BaselineCaptured;
}
