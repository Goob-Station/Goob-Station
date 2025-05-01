// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[Serializable, NetSerializable]
public sealed class PredictedProjectileHitEvent(int projectile, HashSet<(NetEntity Id, MapCoordinates Coordinates)> hit) : EntityEventArgs
{
    public readonly int Projectile = projectile;
    public readonly HashSet<(NetEntity Id, MapCoordinates Coordinates)> Hit = hit;
}
