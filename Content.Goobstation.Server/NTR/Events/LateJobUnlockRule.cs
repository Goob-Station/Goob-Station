// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NTR.Events;

public sealed class LateJobUnlockRule : StationEventSystem<LateJobUnlockRuleComponent>
{
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    protected override void Started(EntityUid uid, LateJobUnlockRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        foreach (var station in _station.GetStationsSet())
        {
            if (!HasComp<StationJobsComponent>(station))
            {
                _chat.SendAdminAlert($"Station {_station.GetOwningStation(station)} has no jobs component. Skipping job unlocks.");
                continue;
            }

            foreach (var (jobProtoId, slotCount) in component.JobsToAdd)
            {
                var jobId = jobProtoId.ToString();

                if (!_prototype.HasIndex(jobProtoId))
                {
                    _chat.SendAdminAlert($"Job prototype '{jobId}' not found for station {_station.GetOwningStation(station)}");
                    continue;
                }

                var currentSlots = _stationJobs.TryGetJobSlot(station, jobId, out var slots) ? slots ?? 0 : 0;
                _stationJobs.TrySetJobSlot(station, jobId, currentSlots + slotCount);
            }
        }
    }
}
