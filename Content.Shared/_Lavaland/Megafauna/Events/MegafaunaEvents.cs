// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

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
