// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.StationEvents.Components;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.StationRecords;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

public sealed class ClericalErrorRule : StationEventSystem<ClericalErrorRuleComponent>
{
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;

    protected override void Started(EntityUid uid, ClericalErrorRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!TryComp<StationRecordsComponent>(chosenStation, out var stationRecords))
            return;

        var recordCount = stationRecords.Records.Keys.Count;

        if (recordCount == 0)
            return;

        var min = (int) Math.Max(1, Math.Round(component.MinToRemove * recordCount));
        var max = (int) Math.Max(min, Math.Round(component.MaxToRemove * recordCount));
        var toRemove = RobustRandom.Next(min, max);
        var keys = new List<uint>();
        for (var i = 0; i < toRemove; i++)
        {
            keys.Add(RobustRandom.Pick(stationRecords.Records.Keys));
        }

        foreach (var id in keys)
        {
            var key = new StationRecordKey(id, chosenStation.Value);
            _stationRecords.RemoveRecord(key, stationRecords);
        }
    }
}