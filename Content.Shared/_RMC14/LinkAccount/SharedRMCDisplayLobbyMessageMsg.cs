// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed class SharedRMCDisplayLobbyMessageEvent(string message, string user) : EntityEventArgs
{
    public readonly string Message = message;
    public readonly string User = user;
}