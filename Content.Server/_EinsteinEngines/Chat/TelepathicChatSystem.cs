// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._EinsteinEngines.Abilities.Psionics;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Drugs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;
using System.Text;

namespace Content.Server.Chat;

/// <summary>
/// Extensions for Telepathic chat stuff
/// </summary>
public sealed partial class TelepathicChatSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    // Butchered the Psychognomist functions since at some point we MIGHT port psionics.
    private IEnumerable<INetChannel> GetPsionicChatClients()
    {
        var psions = Filter.Empty()
            .AddWhereAttachedEntity(IsEligibleForTelepathy)
            .Recipients;

        return psions.Select(p => p.Channel);
    }

    private IEnumerable<INetChannel> GetAdminClients()
    {
        return _adminManager.ActiveAdmins
            .Select(p => p.Channel);
    }

    private bool IsEligibleForTelepathy(EntityUid entity)
    {
        return HasComp<TelepathyComponent>(entity)
            && (!TryComp<MobStateComponent>(entity, out var mobstate) || mobstate.CurrentState == MobState.Alive);
    }

    public void SendTelepathicChat(EntityUid source, string message, bool hideChat)
    {
        if (!IsEligibleForTelepathy(source))
            return;

        var clients = GetPsionicChatClients();
        var admins = GetAdminClients();
        string messageWrap;
        string adminMessageWrap;

        messageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message",
            ("telepathicChannelName", Loc.GetString("chat-manager-telepathic-channel-name")), ("message", message));

        adminMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-admin",
            ("source", source), ("message", message));

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathic chat from {ToPrettyString(source):Player}: {message}");

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, messageWrap, source, hideChat, true, clients.ToList(), Color.PaleVioletRed);

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, adminMessageWrap, source, hideChat, true, admins, Color.PaleVioletRed);
    }
}