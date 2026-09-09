// SPDX-License-Identifier: AGPL-3.0-or-later

// Provides the attributes that allow this event to travel from server to client.
using Robust.Shared.Serialization;

namespace Content.Shared.CombatMode;

/// <summary>
/// Tells one client that their currently controlled entity received positive damage.
/// </summary>
[Serializable, NetSerializable]
public sealed class CombatMusicDamageEvent : EntityEventArgs;
