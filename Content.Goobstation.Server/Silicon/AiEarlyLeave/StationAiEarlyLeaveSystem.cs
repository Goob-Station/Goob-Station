using Content.Goobstation.Shared.Silicon;
using Content.Goobstation.Shared.Silicon.Components;
using Linguini.Bundle.Errors;

using Content.Server.Chat.Systems;
using Robust.Shared.Player;
using Content.Server.EUI;
using Robust.Shared.Network;
using Content.Server.Station.Components;
using Content.Goobstation.Server.Silicons;
using Content.Server.Station.Systems;

public sealed class StationAiEarlyLeaveSystem : SharedStationAiEarlyLeaveSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly StationJobsSystem _jobs = default!;
    [Dependency] private readonly StationSystem _station = default!;



    protected override void RequestEarlyLeave(EntityUid insertedAi)
    {
        if (!_player.TryGetSessionByEntity(insertedAi, out var aiSession))
            return;

        if (aiSession == null)
            return;

        _euiManager.OpenEui(new StationAiEarlyLeaveEui(insertedAi, aiSession.UserId, this), aiSession);
    }
    public void EarlyLeave(EntityUid insertedAi, NetUserId userId)
    {
        var station = _station.GetOwningStation(insertedAi);

        // removes all of player's jobs on all stations
        foreach (var uniqueStation in _station.GetStationsSet())
        {
            if (!TryComp<StationJobsComponent>(uniqueStation, out var stationJobs))
                continue;

            if (!_jobs.TryGetPlayerJobs(uniqueStation, userId, out var jobs, stationJobs))
                continue;

            foreach (var job in jobs)
            {
                _jobs.TryAdjustJobSlot(uniqueStation, job, 1, clamp: true);
            }

            _jobs.TryRemovePlayerJobs(uniqueStation, userId, stationJobs);
        }

        if (station is not { })
            return;

        // lowkirk this is stupid but im uncreative
        _chat.DispatchStationAnnouncement(station.Value,
            Loc.GetString(
                "earlyleave-cryo-announcement",
                ("character", Name(insertedAi)),
                ("entity", insertedAi),
                ("job", "Station AI")
            ), Loc.GetString("earlyleave-cryo-sender"),
            playDefaultSound: false
        );

        QueueDel(insertedAi);
    }
}
