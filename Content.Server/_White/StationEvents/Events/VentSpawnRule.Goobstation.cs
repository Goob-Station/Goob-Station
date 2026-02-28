using Content.Server.Station.Components;

namespace Content.Server._White.StationEvents.Events;

public sealed partial class VentSpawnRule
{
    private bool HasStationGrids(EntityUid stationUid)
    {
        if (!TryComp<StationDataComponent>(stationUid, out var stationData))
            return false;

        foreach (var gridUid in stationData.Grids)
            if (HasComp<BecomesStationComponent>(gridUid))
                return true;

        return false;
    }
}
