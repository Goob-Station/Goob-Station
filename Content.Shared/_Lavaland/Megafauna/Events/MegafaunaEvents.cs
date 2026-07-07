// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Megafauna.Events;

/// <summary>
/// Raised when boss is fully defeated.
/// </summary>
public sealed class MegafaunaKilledEvent : EntityEventArgs;

/// <summary>
/// Raised when MegafaunaAi becomes active and starts calculating logic
/// </summary>
public sealed class MegafaunaStartupEvent : EntityEventArgs;

/// <summary>
/// Raised when boss doesn't die but for any reason deactivates.
/// </summary>
public sealed class MegafaunaShutdownEvent : EntityEventArgs;

/// <summary>
/// Sent to clients whenever a boss's phase changes on the server.
/// This exists because projectile damage is not predicted and never triggers
/// phase switch on sprites.
/// </summary>
[Serializable, NetSerializable]
public sealed class MobPhaseChangedNetworkEvent : EntityEventArgs
{
    public readonly NetEntity Entity;
    public readonly int OldPhase;
    public readonly int NewPhase;

    public MobPhaseChangedNetworkEvent(NetEntity entity, int oldPhase, int newPhase)
    {
        Entity = entity;
        OldPhase = oldPhase;
        NewPhase = newPhase;
    }
}
