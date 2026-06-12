// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Shared._Funkystation.MalfAI;

public static class SharedMalfAiHelpers
{
    /// <summary>
    /// Given an entity that may be a Malf AI or its held brain, resolves
    /// the AI core container entity.
    /// </summary>
    public static EntityUid? ResolveAiCoreFrom(
        IEntityManager entMan,
        SharedTransformSystem xforms,
        EntityUid entity)
    {
        // If this entity directly has a StationAiHeldComponent the parent is the core
        if (entMan.TryGetComponent<StationAiHeldComponent>(entity, out _))
        {
            var xform = entMan.GetComponent<TransformComponent>(entity);
            return xform.ParentUid;
        }

        // If this entity IS a core
        if (entMan.HasComponent<StationAiCoreComponent>(entity))
            return entity;

        return null;
    }
}
