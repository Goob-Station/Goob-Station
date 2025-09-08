// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.FloorGoblin;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared._DV.Abilities;
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
        SubscribeLocalEvent<CrawlUnderFloorComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CrawlUnderFloorComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            var enabled = _appearance.TryGetData(uid, SneakMode.Enabled, out bool apEnabled) && apEnabled;

            if (!enabled)
            {
                if (_lastCell.Remove(uid) && comp.OriginalDrawDepth != null)
                {
                    _sprite.SetDrawDepth((uid, sprite), (int) comp.OriginalDrawDepth);
                    comp.OriginalDrawDepth = null;
                }

                if (sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded((uid, sprite), false);

                continue;
            }

            if (_transform.GetGrid(xform.Coordinates) is not { } gridUid)
                continue;
            if (!TryComp<MapGridComponent>(gridUid, out var grid))
                continue;

            var snapPos = _map.TileIndicesFor((gridUid, grid), xform.Coordinates);
            _lastCell[uid] = (gridUid, snapPos);
            ApplySneakVisuals(uid, comp, sprite);
        }
    }

    private void ApplySneakVisuals(EntityUid uid, CrawlUnderFloorComponent component, SpriteComponent sprite)
    {
        var enabled = _appearance.TryGetData(uid, SneakMode.Enabled, out bool apEnabled) && apEnabled;

        var onSubfloor = IsOnSubfloor(uid);

        if (enabled)
        {
            if (component.OriginalDrawDepth == null)
                component.OriginalDrawDepth = sprite.DrawDepth;

            if (onSubfloor)
            {
                if (sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded((uid, sprite), false);
                _sprite.SetDrawDepth((uid, sprite), (int) DrawDepth.BelowFloor);
            }
            else
            {
                if (!sprite.ContainerOccluded)
                    _sprite.SetContainerOccluded((uid, sprite), true);
            }
        }
        else
        {
            if (component.OriginalDrawDepth != null)
            {
                _sprite.SetDrawDepth((uid, sprite), (int) component.OriginalDrawDepth);
                component.OriginalDrawDepth = null;
            }

            if (sprite.ContainerOccluded)
                _sprite.SetContainerOccluded((uid, sprite), false);
        }
    }

    private void OnAppearanceChange(EntityUid uid, CrawlUnderFloorComponent component, AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        ApplySneakVisuals(uid, component, sprite);
    }
}
