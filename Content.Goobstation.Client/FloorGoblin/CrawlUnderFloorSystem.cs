// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared._DV.Abilities;
using Content.Shared._Starlight.VentCrawling;
using Content.Shared.VentCrawler.Tube.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Map.Components;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Goobstation.Client.FloorGoblin;

public sealed partial class HideUnderFloorAbilitySystem : SharedCrawlUnderFloorSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private readonly Dictionary<EntityUid, (EntityUid Grid, Vector2i Tile)> _lastCell = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CrawlUnderFloorComponent, VentCrawlerComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var vent, out var sprite, out var xform))
        {
            if (vent.InTube)
                continue;

            if (!comp.Enabled)
            {
                if (comp.OriginalDrawDepth != null && sprite.DrawDepth != comp.OriginalDrawDepth)
                    _sprite.SetDrawDepth((uid, sprite), (int) comp.OriginalDrawDepth);
                comp.OriginalDrawDepth = null;

                if (sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded((uid, sprite), false);

                _lastCell.Remove(uid);
                continue;
            }

            if (_transform.GetGrid(xform.Coordinates) is { } gridUid && TryComp<MapGridComponent>(gridUid, out var grid))
            {
                var snapPos = _map.TileIndicesFor((gridUid, grid), xform.Coordinates);
                _lastCell[uid] = (gridUid, snapPos);
            }

            ApplySneakVisuals(uid, comp, sprite);
        }
    }

    private void ApplySneakVisuals(EntityUid uid, CrawlUnderFloorComponent comp, SpriteComponent sprite)
    {
        var onSubfloor = IsOnSubfloor(uid);

        if (comp.Enabled)
        {
            if (comp.OriginalDrawDepth == null)
                comp.OriginalDrawDepth = sprite.DrawDepth;

            if (onSubfloor)
            {
                if (sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded((uid, sprite), false);
                if (sprite.DrawDepth != (int) DrawDepth.BelowFloor)
                    _sprite.SetDrawDepth((uid, sprite), (int) DrawDepth.BelowFloor);
            }
            else
            {
                if (!sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded((uid, sprite), true);
                if (comp.OriginalDrawDepth != null && sprite.DrawDepth != comp.OriginalDrawDepth)
                    _sprite.SetDrawDepth((uid, sprite), (int) comp.OriginalDrawDepth);
            }
        }
        else
        {
            if (comp.OriginalDrawDepth != null && sprite.DrawDepth != comp.OriginalDrawDepth)
                _sprite.SetDrawDepth((uid, sprite), (int) comp.OriginalDrawDepth);
            comp.OriginalDrawDepth = null;

            if (sprite.ContainerOccluded)
                _sprite.SetContainerOccluded((uid, sprite), false);
        }
    }

}
