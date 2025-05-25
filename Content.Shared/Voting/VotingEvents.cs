// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Voting;

[Serializable, NetSerializable]
public sealed class VotePlayerListRequestEvent : EntityEventArgs
{

}

[Serializable, NetSerializable]
public sealed class VotePlayerListResponseEvent : EntityEventArgs
{
    public VotePlayerListResponseEvent((NetUserId, NetEntity, string)[] players, bool denied)
    {
        Players = players;
        Denied = denied;
    }

    /// <summary>
    /// The players available to have a votekick started for them.
    /// </summary>
    public (NetUserId, NetEntity, string)[] Players { get; }

    /// <summary>
    /// Whether the server will allow the user to start a votekick or not.
    /// </summary>
    public bool Denied;
}