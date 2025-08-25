using Content.Shared.StationReport;
using Robust.Shared.GameObjects;

namespace Content.Client.StationReport;

public sealed class StationReportSystem : EntitySystem
{
    // This will store the last received station report
    private string? _stationReportText;

    public string? StationReportText => _stationReportText;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to the network event from the server
        SubscribeNetworkEvent<StationReportEvent>(OnStationReportReceived);
    }

    private void OnStationReportReceived(StationReportEvent ev)
    {
        // Save the received message in the variable
        _stationReportText = ev.StationReportText;
    }
}
