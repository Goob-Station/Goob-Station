// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Lavaland.Weapons.Ranged.Events;

/// <summary>
/// Raised on a gun when a projectile has been fired from it.
/// </summary>
public sealed class ProjectileShotEvent : EntityEventArgs
{
    public EntityUid FiredProjectile = default!;
}

