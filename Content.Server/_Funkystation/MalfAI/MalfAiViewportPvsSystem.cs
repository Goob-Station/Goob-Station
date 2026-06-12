// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Silicons.StationAi;
using Robust.Server.GameStates;
using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// System that ensures entities around MalfAI viewport anchors are included in PVS
/// so they remain visible when the AI travels away from the viewport location.
/// </summary>
/// <remarks>
/// ExpandPvsEvent is raised from the PVS serialization Parallel.For (worker threads),
/// so the handler must not run entity lookups or throw. The entity scan happens on the
/// main thread in Update() and the handler only copies the cached result.
/// </remarks>
public sealed class MalfAiViewportPvsSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// Cached entities near each AI's viewport anchor, refreshed on the main thread.
    /// </summary>
    private readonly Dictionary<EntityUid, List<EntityUid>> _cachedNearby = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationAiHeldComponent, ExpandPvsEvent>(OnExpandPvs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Refreshed every tick: entities deleted between the scan and the PVS send would
        // otherwise produce "Attempted to add non-existent entity to PVS override" errors.
        _cachedNearby.Clear();

        var query = EntityQueryEnumerator<MalfAiViewportComponent, StationAiHeldComponent>();
        while (query.MoveNext(out var uid, out var viewport, out _))
        {
            if (viewport.ViewportAnchor is not { } anchor || !Exists(anchor) || Deleted(anchor))
                continue;

            var anchorCoords = _transform.GetMapCoordinates(anchor);
            if (anchorCoords.MapId == MapId.Nullspace)
                continue;

            // Define visibility radius around viewport anchor (4 tiles radius)
            var visibilityRadius = 4.0f;
            if (_mapManager.TryFindGridAt(anchorCoords, out _, out var grid))
                visibilityRadius *= grid.TileSize;

            var nearby = new List<EntityUid>();
            foreach (var entity in _lookup.GetEntitiesInRange(anchorCoords, visibilityRadius))
            {
                if (entity != anchor)
                    nearby.Add(entity);
            }

            _cachedNearby[uid] = nearby;
        }
    }

    private void OnExpandPvs(EntityUid uid, StationAiHeldComponent component, ref ExpandPvsEvent args)
    {
        // Runs on a PVS worker thread: only copy the precomputed list, never query lookups.
        if (!_cachedNearby.TryGetValue(uid, out var nearby) || nearby.Count == 0)
            return;

        args.Entities ??= new List<EntityUid>();
        foreach (var entity in nearby)
        {
            // Existence check is a thread-safe read; skips entities deleted this tick.
            if (Exists(entity))
                args.Entities.Add(entity);
        }
    }
}
