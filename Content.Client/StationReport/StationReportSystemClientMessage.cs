using Content.Goobstation.Shared.StationReport;
using Robust.Shared.GameObjects;

namespace Content.Client.StationReport;

public sealed class StationReportSystem : EntitySystem
{
    //This is from goob idk why id doesnt work when in goobstation.client
    //stores the last received station report
    private string? _stationReportText;

    public string? StationReportText => _stationReportText;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<StationReportEvent>(OnStationReportReceived);
    }

    private void OnStationReportReceived(StationReportEvent ev)
    {
        //Save the received message in the variable
        _stationReportText = ev.StationReportText;
    }
}
