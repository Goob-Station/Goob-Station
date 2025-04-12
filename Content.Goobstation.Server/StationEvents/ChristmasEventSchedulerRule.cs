// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Southbridge <7013162+southbridge-fur@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spanky <scott@wearejacob.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.Administration.Logs;
using Content.Shared.GameTicking.Components;

namespace Content.Goobstation.Server.StationEvents;

public sealed partial class ChristmasEventSchedulerRule : GameRuleSystem<ChristmasEventSchedulerComponent>
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var events in EntityQuery<ChristmasEventSchedulerComponent>())
        {
            events.EventClock -= frameTime;
            if (events.EventClock <= 0)
            {
                //RollRandomChristmasEvent(events);
                ResetTimer(events);
            }
        }
    }

    private void ResetTimer(ChristmasEventSchedulerComponent component)
    {
        component.EventClock = RobustRandom.NextFloat(component.Delays.Min, component.Delays.Max);
    }

    protected override void Started(EntityUid uid, ChristmasEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        ResetTimer(component);
    }
    protected override void Ended(EntityUid uid, ChristmasEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        ResetTimer(component);
    }
}