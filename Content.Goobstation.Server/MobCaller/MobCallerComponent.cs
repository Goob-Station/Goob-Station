// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Physics;
using Robust.Shared.Prototypes;
using System;

namespace Content.Goobstation.Server.MobCaller;

/// <summary>
/// Makes this entity periodically spawn mobs in space some distance away from us and have them follow it.
/// </summary>
[RegisterComponent]
public sealed partial class MobCallerComponent : Component
{
    /// <summary>
    /// With what periodicity to spawn mobs.
    /// </summary>
    [DataField]
    public TimeSpan SpawnSpacing = TimeSpan.FromSeconds(30);

    /// <summary>
    /// What prototype to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId SpawnProto;

    /// <summary>
    /// Stop spawning if there's that many alive entities spawned by us.
    /// </summary>
    [DataField]
    public int MaxAlive = 5;

    /// <summary>
    /// Amount of spawn directions to consider.
    /// </summary>
    [DataField]
    public int SpawnDirections = 8;

    /// <summary>
    /// Distance to check for occlusions.
    /// Exists for performance reasons, as ideally we'd check up to MaxDistance.
    /// </summary>
    [DataField]
    public float OcclusionDistance = 30f;

    /// <summary>
    /// Collision mask to check occlusion against.
    /// </summary>
    [DataField]
    public CollisionGroup OcclusionMask = CollisionGroup.Impassable;

    /// <summary>
    /// Minimum distance away from us to spawn mobs at.
    /// </summary>
    [DataField]
    public float MinDistance = 50f;

    /// <summary>
    /// Maximum distance away from us to spawn mobs at.
    /// </summary>
    [DataField]
    public float MaxDistance = 120f; // they'll presumably fly to us anyway so can be large

    /// <summary>
    /// Whether to need to be anchored to work.
    /// </summary>
    [DataField]
    public bool NeedAnchored = true;

    /// <summary>
    /// Whether to need power to work.
    /// </summary>
    [DataField]
    public bool NeedPower = true;

    /// <summary>
    /// Progress towards spawning the next entity.
    /// </summary>
    [ViewVariables]
    public TimeSpan SpawnAccumulator = TimeSpan.FromSeconds(0);

    /// <summary>
    /// Entities spawned by us.
    /// Used to keep track of how many are alive.
    /// Entities that are dead will be removed from this list.
    /// </summary>
    [ViewVariables]
    public List<EntityUid> SpawnedEntities = new();
}
