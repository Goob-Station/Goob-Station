// SPDX-FileCopyrightText: 2026 Goob-Station Contributors <https://github.com/Goob-Station/Goob-Station>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Goobstation.Common.Wanted;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.StationEvents.Events;

/// <summary>
/// Station event that identifies all highly notorious criminals on the station and
/// broadcasts a wanted announcement listing their names and bounties over station comms.
/// Aborts silently if there are not enough notorious criminals to meet the threshold.
/// </summary>
public sealed class ManhuntRule : StationEventSystem<ManhuntRuleComponent>
{
    protected override void Started(EntityUid uid, ManhuntRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Gather all criminals that meet the minimum notoriety threshold.
        var criminals = new List<(string Name, int Level, int Bounty)>();
        var totalNotoriety = 0;

        var query = EntityQueryEnumerator<NotorietyComponent>();
        while (query.MoveNext(out var entity, out var notoriety))
        {
            totalNotoriety += notoriety.Level;

            if (notoriety.Level >= component.MinNotorietyLevel)
                criminals.Add((Name(entity), notoriety.Level, notoriety.BountyAmount));
        }

        // Abort if there are not enough notorious criminals to justify a manhunt.
        if (criminals.Count == 0 || totalNotoriety < component.MinTotalNotoriety)
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        // Sort by level descending and cap the list.
        criminals.Sort((a, b) => b.Level.CompareTo(a.Level));
        if (criminals.Count > component.MaxListed)
            criminals.RemoveRange(component.MaxListed, criminals.Count - component.MaxListed);

        // Build the criminal listing.
        var sb = new StringBuilder();
        foreach (var (name, level, bounty) in criminals)
        {
            sb.AppendLine(Loc.GetString("manhunt-criminal-entry",
                ("name", name),
                ("level", level),
                ("bounty", bounty)));
        }

        var announcement = Loc.GetString("manhunt-announcement",
            ("criminals", sb.ToString().TrimEnd()));

        var allPlayers = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
        ChatSystem.DispatchFilteredAnnouncement(allPlayers, announcement, playSound: false, colorOverride: Color.OrangeRed);
    }
}