using Content.Server.GameTicking;
using Content.Shared.StationReport;
using Content.Shared.Paper;
using Robust.Shared.GameObjects;

namespace Content.Server.StationReportSystem;

public sealed class StationReportSystem : EntitySystem
{
    public string? StationReportText;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        var query = EntityQueryEnumerator<StationReportTabletComponent>();
        while (query.MoveNext(out var uid, out var tablet))
        {
            if (EntityManager.TryGetComponent<PaperComponent>(uid, out var paper))
            {
                StationReportText = paper.Content;
                break;
            }
        }
        BroadcastStationReport(StationReportText);
    }

    public void BroadcastStationReport(string? text)
    {
        RaiseNetworkEvent(new StationReportEvent(text));
    }
}
