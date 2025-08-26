using Content.Server.GameTicking;
using Content.Shared.StationReport;
using Content.Shared.Paper;
using Robust.Shared.GameObjects;

namespace Content.Server.StationReportSystem;

public sealed class StationReportSystem : EntitySystem
{

    //this is shitcode
    public string? StationReportText;

    public override void Initialize()
    {
        //sets stationreporttext to null so it doesn't keep from the previous round
        base.Initialize();
        if (StationReportText != null)
        {
            StationReportText = null;
        }
        //subscribes to the endroundevent
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        //locates the first entity with StationReportTabletComponent then stops
        var query = EntityQueryEnumerator<StationReportTabletComponent>();
        while (query.MoveNext(out var uid, out var tablet))
        {
            if (TryComp<PaperComponent>(uid, out var paper))
            {
                StationReportText = paper.Content;
                break;
            }
        }
        BroadcastStationReport(StationReportText);
    }

    //sends a networkevent to tell the client to update the stationreporttext when recived
    public void BroadcastStationReport(string? text)
    {
        RaiseNetworkEvent(new StationReportEvent(text));
        StationReportText = null;
    }
}
