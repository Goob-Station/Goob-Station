// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.FloorGoblin;
using Content.Shared._DV.Abilities;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Goobstation.Shared.FloorGoblin;
public abstract class SharedCrawlUnderFloorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderFloorComponent, CrawlingUpdatedEvent>(OnCrawlingUpdated);
    }

    private void OnCrawlingUpdated(EntityUid uid,
        CrawlUnderFloorComponent component,
        CrawlingUpdatedEvent args)
    {
        if (args.Enabled)
            _popup.PopupEntity(Loc.GetString("crawl-under-floor-toggle-on"), uid);
        else
            _popup.PopupEntity(Loc.GetString("crawl-under-floor-toggle-off"), uid);
    }

    public bool IsOnCollidingTile(EntityUid uid)
    {
        var xform = Transform(uid);
        if (xform.Coordinates.GetTileRef() is not { } tile)
            return false;
        return _turf.IsTileBlocked(tile, CollisionGroup.SmallMobMask);
    }

    public bool IsOnSubfloor(EntityUid uid)
    {
        var xform = Transform(uid);
        if (_transform.GetGrid(xform.Coordinates) is not { } gridUid)
            return false;
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return false;
        var snapPos = _map.TileIndicesFor((gridUid, grid), xform.Coordinates);
        var tileRef = _map.GetTileRef(gridUid, grid, snapPos);
        if (tileRef.Tile.IsEmpty)
            return false;
        var tileDef = (ContentTileDefinition) _tileManager[tileRef.Tile.TypeId];
        return tileDef.IsSubFloor;
    }

    public bool IsHidden(EntityUid uid, CrawlUnderFloorComponent comp)
    => comp.Enabled && !IsOnSubfloor(uid);
}
