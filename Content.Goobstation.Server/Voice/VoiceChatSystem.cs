// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Voice;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceChatSystem : EntitySystem
{
    [Dependency] private readonly IVoiceChatServerManager _voiceChatManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (_voiceChatManager is not VoiceChatServerManager voiceChatServerManager)
            return;

        var playerEndpoint = ev.Player.Channel.RemoteEndPoint.Address;
        foreach (var clientData in voiceChatServerManager.Clients.Values)
        {
            if (clientData.Connection.RemoteEndPoint.Address.Equals(playerEndpoint))
            {
                if (clientData.PlayerEntity == ev.Entity)
                    return;

                Logger.Debug($"Player {ev.Player.Name} attached to new entity {ev.Entity}. Updating voice client data.");
                clientData.PlayerEntity = ev.Entity;
                break;
            }
        }
    }
}
