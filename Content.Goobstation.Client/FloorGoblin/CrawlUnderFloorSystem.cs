// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 portfiend <109661617+portfiend@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Content.Goobstation.Common.FloorGoblin;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared._DV.Abilities;
using Content.Shared.Maps;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.Goobstation.Client.FloorGoblin;

public sealed partial class HideUnderFloorAbilitySystem : SharedCrawlUnderFloorSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;

    private readonly Dictionary<EntityUid, (EntityUid Grid, Vector2i Tile)> _lastCell = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlUnderFloorComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<MapGridComponent, TileChangedEvent>(OnTileChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<CrawlUnderFloorComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            _appearance.TryGetData(uid, SneakMode.Enabled, out bool enabled);

            if (!enabled)
            {
                if (sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded(uid, false);

                continue;
            }

            var gridUid = _transform.GetGrid(xform.Coordinates);
            if (gridUid == null)
                continue;
            if (!TryComp<MapGridComponent>(gridUid.Value, out var grid))
                continue;

            var snapPos = _map.TileIndicesFor((gridUid.Value, grid), xform.Coordinates);

            if (!_lastCell.TryGetValue(uid, out var last) || last.Grid != gridUid.Value || last.Tile != snapPos)
            {
                _lastCell[uid] = (gridUid.Value, snapPos);
                ApplySneakVisuals(uid, comp, sprite);
            }
        }
    }

    private bool IsOnSubfloor(EntityUid uid)
    {
        var xform = Transform(uid);
        var gridUid = _transform.GetGrid(xform.Coordinates);
        if (gridUid == null)
            return false;
        if (!TryComp<MapGridComponent>(gridUid.Value, out var grid))
            return false;
        var snapPos = _map.TileIndicesFor((gridUid.Value, grid), xform.Coordinates);
        var tileRef = _map.GetTileRef(gridUid.Value, grid, snapPos);
        if (tileRef.Tile.IsEmpty)
            return false;
        var tileDef = (ContentTileDefinition) _tileManager[tileRef.Tile.TypeId];
        return tileDef.IsSubFloor;
    }

    private void ApplySneakVisuals(EntityUid uid, CrawlUnderFloorComponent component, SpriteComponent sprite)
    {
        _appearance.TryGetData(uid, SneakMode.Enabled, out bool enabled);
        var onSubfloor = IsOnSubfloor(uid);

        if (enabled)
        {

            if (onSubfloor)
            {
                if (sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded(uid, false);
            }
            else
            {
                if (!sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded(uid, true);
            }
        }
        else
        {

            if (sprite.ContainerOccluded)
                _sprite.SetContainerOccluded(uid, false);
        }
    }

    private void OnAppearanceChange(EntityUid uid, CrawlUnderFloorComponent component, AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        ApplySneakVisuals(uid, component, sprite);
    }

    private void OnTileChanged(EntityUid gridUid, MapGridComponent grid, ref TileChangedEvent args)
    {
        var query = EntityQueryEnumerator<CrawlUnderFloorComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            var g = _transform.GetGrid(xform.Coordinates);
            if (g == null || g != gridUid)
                continue;

            ApplySneakVisuals(uid, comp, sprite);
            var snapPos = _map.TileIndicesFor((gridUid, grid), xform.Coordinates);
            _lastCell[uid] = (gridUid, snapPos);
        }
    }
}
