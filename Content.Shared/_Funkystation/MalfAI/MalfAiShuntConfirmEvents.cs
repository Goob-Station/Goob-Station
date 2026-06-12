// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Server to client: shunting now would abort the active doomsday protocol, ask for confirmation.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiShuntConfirmRequestEvent : EntityEventArgs
{
    public NetEntity Apc;

    public MalfAiShuntConfirmRequestEvent(NetEntity apc)
    {
        Apc = apc;
    }
}

/// <summary>
/// Client to server: the player confirmed the shunt despite the doomsday abort.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiShuntConfirmResponseEvent : EntityEventArgs
{
    public NetEntity Apc;

    public MalfAiShuntConfirmResponseEvent(NetEntity apc)
    {
        Apc = apc;
    }
}
