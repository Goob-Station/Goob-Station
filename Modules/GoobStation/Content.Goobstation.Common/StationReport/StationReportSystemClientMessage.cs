using Content.Goobstation.Common.StationReport;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.StationReport;

public sealed class StationReportSystem : EntitySystem
{
    //stores the last received station report
    public string? StationReportText { get; private set; } = null;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<StationReportEvent>(OnStationReportReceived);
    }

    private void OnStationReportReceived(StationReportEvent ev)
    {
        //Save the received message in the variable
        StationReportText = ev.StationReportText;
    }
}
