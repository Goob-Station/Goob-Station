// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Myra <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 PJB3005 <pieterjan.briers+git@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading.Tasks;
using Content.Server.Connection.Whitelist;
using Content.Server.Connection.Whitelist.Conditions;
using Content.Server.Database;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Network;

namespace Content.Server.Connection;

/// <summary>
/// Handles whitelist conditions for incoming connections.
/// </summary>
public sealed partial class ConnectionManager
{
    private PlayerConnectionWhitelistPrototype[]? _whitelists;

    private void InitializeWhitelist()
    {
        _cfg.OnValueChanged(CCVars.WhitelistPrototypeList, UpdateWhitelists, true);
    }

    private void UpdateWhitelists(string s)
    {
        var list = new List<PlayerConnectionWhitelistPrototype>();
        foreach (var id in s.Split(','))
        {
            if (_prototypeManager.TryIndex(id, out PlayerConnectionWhitelistPrototype? prototype))
            {
                list.Add(prototype);
            }
            else
            {
                _sawmill.Fatal($"Whitelist prototype {id} does not exist. Denying all connections.");
                _whitelists = null; // Invalidate the list, causes deny on all connections.
                return;
            }
        }

        _whitelists = list.ToArray();
    }

    private bool IsValid(PlayerConnectionWhitelistPrototype whitelist, int playerCount)
    {
        return playerCount >= whitelist.MinimumPlayers && playerCount <= whitelist.MaximumPlayers;
    }

    public async Task<(bool isWhitelisted, string? denyMessage)> IsWhitelisted(PlayerConnectionWhitelistPrototype whitelist, NetUserData data, ISawmill sawmill)
    {
        var cacheRemarks = await _db.GetAllAdminRemarks(data.UserId);
        var cachePlaytime = await _db.GetPlayTimes(data.UserId);

        foreach (var condition in whitelist.Conditions)
        {
            bool matched;
            string denyMessage;
            switch (condition)
            {
                case ConditionAlwaysMatch:
                    matched = true;
                    denyMessage = Loc.GetString("whitelist-always-deny");
                    break;
                case ConditionManualWhitelistMembership:
                    matched = await CheckConditionManualWhitelist(data);
                    denyMessage = Loc.GetString("whitelist-manual");
                    break;
                case ConditionManualBlacklistMembership:
                    matched = await CheckConditionManualBlacklist(data);
                    denyMessage = Loc.GetString("whitelist-blacklisted");
                    break;
                case ConditionNotesDateRange conditionNotes:
                    matched = CheckConditionNotesDateRange(conditionNotes, cacheRemarks);
                    denyMessage = Loc.GetString("whitelist-notes");
                    break;
                case ConditionPlayerCount conditionPlayerCount:
                    matched = CheckConditionPlayerCount(conditionPlayerCount);
                    denyMessage = Loc.GetString("whitelist-player-count");
                    break;
                case ConditionPlaytime conditionPlaytime:
                    matched = CheckConditionPlaytime(conditionPlaytime, cachePlaytime);
                    denyMessage = Loc.GetString("whitelist-playtime", ("minutes", conditionPlaytime.MinimumPlaytime));
                    break;
                case ConditionNotesPlaytimeRange conditionNotesPlaytimeRange:
                    matched = CheckConditionNotesPlaytimeRange(conditionNotesPlaytimeRange, cacheRemarks, cachePlaytime);
                    denyMessage = Loc.GetString("whitelist-notes");
                    break;
                default:
                    throw new NotImplementedException($"Whitelist condition {condition.GetType().Name} not implemented");
            }

            sawmill.Verbose($"User {data.UserName} whitelist condition {condition.GetType().Name} result: {matched}");
            sawmill.Verbose($"Action: {condition.Action.ToString()}");
            switch (condition.Action)
            {
                case ConditionAction.Allow:
                    if (matched)
                    {
                        sawmill.Verbose($"User {data.UserName} passed whitelist condition {condition.GetType().Name} and it's a breaking condition");
                        return (true, denyMessage);
                    }
                    break;
                case ConditionAction.Deny:
                    if (matched)
                    {
                        sawmill.Verbose($"User {data.UserName} failed whitelist condition {condition.GetType().Name}");
                        return (false, denyMessage);
                    }
                    break;
                default:
                    sawmill.Verbose($"User {data.UserName} failed whitelist condition {condition.GetType().Name} but it's not a breaking condition");
                    break;
            }
        }
        sawmill.Verbose($"User {data.UserName} passed all whitelist conditions");
        return (true, null);
    }

    #region Condition Checking

    private async Task<bool> CheckConditionManualWhitelist(NetUserData data)
    {
        return await _db.GetWhitelistStatusAsync(data.UserId);
    }

    private async Task<bool> CheckConditionManualBlacklist(NetUserData data)
    {
        return await _db.GetBlacklistStatusAsync(data.UserId);
    }

    private bool CheckConditionNotesDateRange(ConditionNotesDateRange conditionNotes, List<IAdminRemarksRecord> remarks)
    {
        var range = DateTime.UtcNow.AddDays(-conditionNotes.Range);

        return CheckRemarks(remarks,
            conditionNotes.IncludeExpired,
            conditionNotes.IncludeSecret,
            conditionNotes.MinimumSeverity,
            conditionNotes.MinimumNotes,
            adminRemarksRecord => adminRemarksRecord.CreatedAt > range);
    }

    private bool CheckConditionPlayerCount(ConditionPlayerCount conditionPlayerCount)
    {
        var count = _plyMgr.PlayerCount;
        return count >= conditionPlayerCount.MinimumPlayers && count <= conditionPlayerCount.MaximumPlayers;
    }

    private bool CheckConditionPlaytime(ConditionPlaytime conditionPlaytime, List<PlayTime> playtime)
    {
        var tracker = playtime.Find(p => p.Tracker == PlayTimeTrackingShared.TrackerOverall);
        if (tracker is null)
        {
            return false;
        }

        return tracker.TimeSpent.TotalMinutes >= conditionPlaytime.MinimumPlaytime;
    }

    private bool CheckConditionNotesPlaytimeRange(
        ConditionNotesPlaytimeRange conditionNotesPlaytimeRange,
        List<IAdminRemarksRecord> remarks,
        List<PlayTime> playtime)
    {
        var overallTracker = playtime.Find(p => p.Tracker == PlayTimeTrackingShared.TrackerOverall);
        if (overallTracker is null)
        {
            return false;
        }

        return CheckRemarks(remarks,
            conditionNotesPlaytimeRange.IncludeExpired,
            conditionNotesPlaytimeRange.IncludeSecret,
            conditionNotesPlaytimeRange.MinimumSeverity,
            conditionNotesPlaytimeRange.MinimumNotes,
            adminRemarksRecord => adminRemarksRecord.PlaytimeAtNote >= overallTracker.TimeSpent - TimeSpan.FromMinutes(conditionNotesPlaytimeRange.Range));
    }

    private bool CheckRemarks(List<IAdminRemarksRecord> remarks, bool includeExpired, bool includeSecret, NoteSeverity minimumSeverity, int MinimumNotes, Func<IAdminRemarksRecord, bool> additionalCheck)
    {
        var utcNow = DateTime.UtcNow;

        var notes = remarks.Count(r => r is AdminNoteRecord note && note.Severity >= minimumSeverity && (includeSecret || !note.Secret) && (includeExpired || note.ExpirationTime == null || note.ExpirationTime > utcNow));
        if (notes < MinimumNotes)
        {
            return false;
        }

        foreach (var adminRemarksRecord in remarks)
        {
            // If we're not including expired notes, skip them
            if (!includeExpired && (adminRemarksRecord.ExpirationTime == null || adminRemarksRecord.ExpirationTime <= utcNow))
                continue;

            // In order to get the severity of the remark, we need to see if it's an AdminNoteRecord.
            if (adminRemarksRecord is not AdminNoteRecord adminNoteRecord)
                continue;

            // We want to filter out secret notes if we're not including them.
            if (!includeSecret && adminNoteRecord.Secret)
                continue;

            // At this point, we need to remove the note if it's not within the severity range.
            if (adminNoteRecord.Severity < minimumSeverity)
                continue;

            // Perform the additional check specific to each method
            if (!additionalCheck(adminRemarksRecord))
                continue;

            // If we've made it this far, we have a match
            return true;
        }

        // No matches
        return false;
    }

    #endregion
}