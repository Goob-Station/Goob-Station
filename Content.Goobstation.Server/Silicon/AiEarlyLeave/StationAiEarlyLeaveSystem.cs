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
using Content.Server.Radio.EntitySystems;
using Robust.Shared.Prototypes;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Server.Radio.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Ghost;

public sealed class StationAiEarlyLeaveSystem : SharedStationAiEarlyLeaveSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly StationJobsSystem _jobs = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly string _alertChannelName = "Command";

    protected override void RequestEarlyLeave(Entity<StationAiCoreComponent> aiCore, EntityUid insertedAi)
    {
        if (!_player.TryGetSessionByEntity(insertedAi, out var aiSession))
            return;

        if (aiSession == null)
            return;

        _euiManager.OpenEui(new StationAiEarlyLeaveEui(aiCore, insertedAi, aiSession.UserId, this), aiSession);
    }

    public void EarlyLeave(Entity<StationAiCoreComponent> aiCore, EntityUid insertedAi, NetUserId userId)
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

        var channel = _prototypeManager.Index<RadioChannelPrototype>(_alertChannelName);

        var filter = Filter.Empty();
        var radioQuery = EntityQueryEnumerator<ActiveRadioComponent, TransformComponent>();

        // get people with access to the radio
        // me when no separate function for checking radio access in RadioSystem
        while (radioQuery.MoveNext(out var receiver, out var radio, out var transform))
        {
            if (!radio.ReceiveAllChannels)
            {
                if (!radio.Channels.Contains(channel.ID) 
                || (TryComp<IntercomComponent>(receiver, out var intercom)
                && !intercom.SupportedChannels.Contains(channel.ID)))
                    continue;
            }

            var parent = transform.ParentUid;

            if (TryComp(parent, out ActorComponent? actor))
                filter.AddPlayer(actor.PlayerSession);
        }
        // also add ghosts its probably fine
        var ghostQuery = EntityQueryEnumerator<GhostComponent>();
        while (ghostQuery.MoveNext(out var ghost, out var _))
        {
            if (TryComp<ActorComponent>(ghost, out var actor))
            {
                filter.AddPlayer(actor.PlayerSession);
            }
        }

        // filtered announcement cuz just not good to announce that ai is offline to literally everyone IC
        _chat.DispatchFilteredAnnouncement(filter,
            Loc.GetString(
                "station-ai-earlyleave-announcement",
                ("character", Name(insertedAi)),
                ("entity", insertedAi)
            ), insertedAi, Loc.GetString("station-ai-earlyleave-announcement-sender")
        );

        QueueDel(insertedAi);
    }
}
