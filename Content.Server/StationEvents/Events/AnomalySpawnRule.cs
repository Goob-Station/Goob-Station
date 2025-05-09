// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Anomaly;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
﻿using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents.Events;

public sealed class AnomalySpawnRule : StationEventSystem<AnomalySpawnRuleComponent>
{
    [Dependency] private readonly AnomalySystem _anomaly = default!;

    protected override void Added(EntityUid uid, AnomalySpawnRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        var str = Loc.GetString("anomaly-spawn-event-announcement",
            ("sighting", Loc.GetString($"anomaly-spawn-sighting-{RobustRandom.Next(1, 6)}")));
        stationEvent.StartAnnouncement = str;

        base.Added(uid, component, gameRule, args);
    }

    protected override void Started(EntityUid uid, AnomalySpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!TryComp<StationDataComponent>(chosenStation, out var stationData))
            return;

        var grid = StationSystem.GetLargestGrid(stationData);

        if (grid is null)
            return;

        var amountToSpawn = 1;
        for (var i = 0; i < amountToSpawn; i++)
        {
            _anomaly.SpawnOnRandomGridLocation(grid.Value, component.AnomalySpawnerPrototype);
        }
    }
}