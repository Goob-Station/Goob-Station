// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.BlueSpaceStorm;

/// <summary>
/// Fires when a portal pulses
/// </summary>
public sealed partial class PortalPulseEvent : EntityEventArgs;

/// <summary>
/// Fires when a portal spawns mobs
/// </summary>
public sealed partial class PortalMobSpawnEvent : EntityEventArgs;

/// <summary>
/// Fires when all of a portal's mobs die
/// </summary>
public sealed partial class PortalMobsAllDeathEvent : EntityEventArgs;
