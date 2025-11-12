// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 jjtParadox <jjtParadox@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Robust.Shared.Random;

// Goobstation usings
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Shared.Database;


namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class BureaucraticErrorRule : StationEventSystem<BureaucraticErrorRuleComponent>
{
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!; // Goob
    [Dependency] private readonly IChatManager _chat = default!; // Goob

    protected override void Started(EntityUid uid, BureaucraticErrorRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation, HasComp<StationJobsComponent>))
            return;

        var jobList = _stationJobs.GetJobs(chosenStation.Value).Keys.ToList();

        foreach(var job in component.IgnoredJobs)
            jobList.Remove(job);

        if (jobList.Count == 0)
            return;

        #region Goobstation changes
        // Fully re-do how this event works, to be less shit,
        // This fully replaces the code.
        // Moderate chance to significantly change the late-join landscape
        if (RobustRandom.Prob(0.25f))
        {
            // Ensure we have at least one job to make unlimited
            if (jobList.Count == 0)
                return;

            // Pick a job to make unlimited
            var chosenJob = RobustRandom.PickAndTake(jobList);
            _stationJobs.MakeJobUnlimited(chosenStation.Value, chosenJob);
            _chat.SendAdminAlert($"Bureaucratic Error: Made {chosenJob} unlimited");
            _adminLog.Add(LogType.AdminMessage, LogImpact.Low, $"Bureaucratic Error: Made {chosenJob} unlimited");

            // Only modify up to 40% of jobs, ensuring 60% remain unchanged
            var jobsToModify = jobList
                .Where(job => !_stationJobs.IsJobUnlimited(chosenStation.Value, job))
                .OrderBy(_ => RobustRandom.Next())
                .Take((int)Math.Ceiling(jobList.Count * 0.4f)) // 40% of jobs
                .ToList();

            foreach (var job in jobsToModify)
            {
                if (!_stationJobs.TryGetJobSlot(chosenStation.Value, job, out var currentSlots) || !currentSlots.HasValue || currentSlots <= 0)
                    continue;

                // Adjust slots with smaller, more balanced changes
                var adjustment = RobustRandom.Next(-1, 2); // -1, 0, or +1
                var newSlots = Math.Max(1, currentSlots.Value + adjustment);
                _stationJobs.TrySetJobSlot(chosenStation.Value, job, newSlots);
                _chat.SendAdminAlert($"Bureaucratic Error: Changed {job} slots from {currentSlots} to {newSlots}");
                _adminLog.Add(LogType.AdminMessage, LogImpact.Low, $"Bureaucratic Error: Changed {job} slots from {currentSlots} to {newSlots}");
            }
        }
        else
        {
            // Change 40% of jobs, ensuring 60% remain unchanged
            var num = Math.Max(1, (int)Math.Ceiling(jobList.Count * 0.4f)); // 40% of jobs, at least 1
            for (var i = 0; i < num; i++)
            {
                var chosenJob = RobustRandom.PickAndTake(jobList);
                if (_stationJobs.IsJobUnlimited(chosenStation.Value, chosenJob))
                    continue;

                // Use very conservative adjustments (-1 to +1) and ensure we don't go below 1 slot
                if (_stationJobs.TryGetJobSlot(chosenStation.Value, chosenJob, out var currentSlots) && currentSlots.HasValue && currentSlots > 0)
                {
                    var adjustment = RobustRandom.Next(-1, 2); // -1, 0, or +1
                    var newSlots = Math.Max(1, currentSlots.Value + adjustment);
                    _stationJobs.TrySetJobSlot(chosenStation.Value, chosenJob, newSlots);
                    _chat.SendAdminAlert($"Bureaucratic Error: Changed {chosenJob} slots from {currentSlots} to {newSlots}");
                    _adminLog.Add(LogType.AdminMessage, LogImpact.Low, $"Bureaucratic Error: Changed {chosenJob} slots from {currentSlots} to {newSlots}");
                }
            }
        }
        #endregion
    }
}
