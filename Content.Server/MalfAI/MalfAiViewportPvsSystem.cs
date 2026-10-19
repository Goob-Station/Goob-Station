// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server.MalfAI;

/// <summary>
/// System that ensures entities around MalfAI viewport anchors are included in PVS
/// so they remain visible when the AI travels away from the viewport location.
/// </summary>
public sealed class MalfAiViewportPvsSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationAiHeldComponent, ExpandPvsEvent>(OnExpandPvs);
    }

    private void OnExpandPvs(EntityUid uid, StationAiHeldComponent component, ref ExpandPvsEvent args)
    {
        // Only process for AI entities with viewport components
        if (!TryComp<MalfAiViewportComponent>(uid, out var viewportComp))
            return;

        // Only if viewport anchor exists and is valid
        if (viewportComp.ViewportAnchor == null || !EntityManager.EntityExists(viewportComp.ViewportAnchor.Value))
            return;

        var anchorUid = viewportComp.ViewportAnchor.Value;
        var anchorCoords = _transform.GetMapCoordinates(anchorUid);

        // Define visibility radius around viewport anchor (4 tiles radius)
        var visibilityRadius = 4.0f;

        // Get grid tile size for proper radius calculation
        if (_mapManager.TryFindGridAt(anchorCoords, out var gridUid, out var grid))
        {
            visibilityRadius *= grid.TileSize;
        }

        // Find all entities within the viewport anchor's visibility radius
        var nearbyEntities = _lookup.GetEntitiesInRange(anchorCoords, visibilityRadius);

        // Initialize the entities list if it doesn't exist
        args.Entities ??= new List<EntityUid>();

        // Add all nearby entities to the PVS expansion list
        foreach (var entity in nearbyEntities)
        {
            // Skip the anchor entity itself to avoid redundancy
            if (entity == anchorUid)
                continue;

            // Add entity to PVS expansion
            args.Entities.Add(entity);
        }
    }
}
