// SPDX-License-Identifier: MIT

using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;

namespace Content.Shared.MalfAI;

public static class SharedMalfAiHelpers
{
    /// <summary>
    /// Resolve the AI core entity associated with the given performer.
    /// Handles direct core, remote entity mappings, and parent-chain traversal.
    /// Returns EntityUid.Invalid if none found.
    /// </summary>
    public static EntityUid ResolveAiCoreFrom(IEntityManager entMan, SharedTransformSystem xform, EntityUid performer)
    {
        if (entMan.HasComponent<StationAiCoreComponent>(performer))
            return performer;

        // Try find a core whose RemoteEntity equals the performer (remote AI control).
        var query = entMan.EntityQueryEnumerator<StationAiCoreComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (comp.RemoteEntity == performer)
                return ent;
        }

        // Walk up the transform chain in case the performer is inserted into the core.
        var current = performer;
        for (var i = 0; i < 8; i++)
        {
            var parent = entMan.GetComponent<TransformComponent>(current).ParentUid;
            if (!entMan.EntityExists(parent))
                break;

            if (entMan.HasComponent<StationAiCoreComponent>(parent))
                return parent;

            current = parent;
        }

        return EntityUid.Invalid;
    }
}
