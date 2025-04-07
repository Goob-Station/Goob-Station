// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Decals.Overlays;

public sealed class DecalPlacementOverlay : Overlay
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    private readonly DecalPlacementSystem _placement;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    public DecalPlacementOverlay(DecalPlacementSystem placement, SharedTransformSystem transform, SpriteSystem sprite)
    {
        IoCManager.InjectDependencies(this);
        _placement = placement;
        _transform = transform;
        _sprite = sprite;
        ZIndex = 1000;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var (decal, snap, rotation, color) = _placement.GetActiveDecal();

        if (decal == null)
            return;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.PixelToMap(mouseScreenPos);

        if (mousePos.MapId != args.MapId)
            return;

        // No map support for decals
        if (!_mapManager.TryFindGridAt(mousePos, out var gridUid, out var grid))
        {
            return;
        }

        var worldMatrix = _transform.GetWorldMatrix(gridUid);
        var invMatrix = _transform.GetInvWorldMatrix(gridUid);

        var handle = args.WorldHandle;
        handle.SetTransform(worldMatrix);

        var localPos = Vector2.Transform(mousePos.Position, invMatrix);

        if (snap)
        {
            localPos = localPos.Floored() + grid.TileSizeHalfVector;
        }

        // Nothing uses snap cardinals so probably don't need preview?
        var aabb = Box2.UnitCentered.Translated(localPos);
        var box = new Box2Rotated(aabb, rotation, localPos);

        handle.DrawTextureRect(_sprite.Frame0(decal.Sprite), box, color);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
