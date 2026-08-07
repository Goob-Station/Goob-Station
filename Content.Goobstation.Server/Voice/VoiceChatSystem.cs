// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Voice;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceChatSystem : EntitySystem
{
    [Dependency] private readonly IVoiceChatServerManager _voiceChatManager = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("voice_chat");
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

                _sawmill.Debug($"Player {ev.Player.Name} attached to new entity {ev.Entity}. Updating voice client data.");
                clientData.PlayerEntity = ev.Entity;
                break;
            }
        }
    }
}
