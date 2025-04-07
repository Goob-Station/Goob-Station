// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared._RMC14.LinkAccount;

namespace Content.Client._RMC14.LinkAccount;

public sealed class LinkAccountSystem : EntitySystem
{
    public event Action<SharedRMCDisplayLobbyMessageEvent>? LobbyMessageReceived;

    public override void Initialize()
    {
        SubscribeNetworkEvent<SharedRMCDisplayLobbyMessageEvent>(OnDisplayLobbyMessage);
    }

    private void OnDisplayLobbyMessage(SharedRMCDisplayLobbyMessageEvent ev)
    {
        LobbyMessageReceived?.Invoke(ev);
    }
}