// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Players.RateLimiting;
using Robust.Shared.Player;

namespace Content.Server.Chat.Managers;

internal sealed partial class ChatManager
{
    private const string RateLimitKey = "Chat";

    private void RegisterRateLimits()
    {
        _rateLimitManager.Register(RateLimitKey,
            new RateLimitRegistration(CCVars.ChatRateLimitPeriod,
                CCVars.ChatRateLimitCount,
                RateLimitPlayerLimited,
                CCVars.ChatRateLimitAnnounceAdminsDelay,
                RateLimitAlertAdmins,
                LogType.ChatRateLimited)
            );
    }

    private void RateLimitPlayerLimited(ICommonSession player)
    {
        DispatchServerMessage(player, Loc.GetString("chat-manager-rate-limited"), suppressLog: true);
    }

    private void RateLimitAlertAdmins(ICommonSession player)
    {
        SendAdminAlert(Loc.GetString("chat-manager-rate-limit-admin-announcement", ("player", player.Name)));
    }

    public RateLimitStatus HandleRateLimit(ICommonSession player)
    {
        return _rateLimitManager.CountAction(player, RateLimitKey);
    }
}