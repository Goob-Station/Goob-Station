// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI.Factory;

/// <summary>
/// Sent from client to server when the player confirms factory placement via the ghost.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiFactoryBuildNetEvent : EntityEventArgs
{
    public NetEntity Performer;
    public NetCoordinates Target;

    public MalfAiFactoryBuildNetEvent(NetEntity performer, NetCoordinates target)
    {
        Performer = performer;
        Target = target;
    }
}
