// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Station.Components;
using Content.Server.StationEvents.Events;
using Content.Server.Station.Components;

namespace Content.Goobstation.Server.NTR.Events;

public sealed class LateJobUnlockRule : StationEventSystem<LateJobUnlockRuleComponent>
{
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationSystem _station = default!;

    protected override void Started(EntityUid uid, LateJobUnlockRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        foreach (var station in _station.GetStationsSet())
        {
            foreach (var (jobProtoId, slotCount) in component.JobsToAdd)
            {
                var jobId = jobProtoId.ToString();

                if (!_prototype.HasIndex<JobPrototype>(jobProtoId))
                    continue;
                var currentSlots = _stationJobs.TryGetJobSlot(station, jobId, out var slots) ? slots ?? 0 : 0;
                _stationJobs.TrySetJobSlot(station, jobId, (int)(currentSlots + slotCount));
            }
        }
    }
}
