// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.CCVar; // Goobstation - MalfAI camera upgrade
using Content.Shared._Funkystation.MalfAI; // Goobstation - MalfAI camera upgrade
using Content.Shared.Silicons.StationAi;
using Content.Shared.SurveillanceCamera; // Goobstation - MalfAI camera upgrade
using Robust.Client.GameObjects; // Goobstation - MalfAI camera upgrade
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Movement.Components; // Shitmed - Starlight Abductors Change

namespace Content.Client.Silicons.StationAi;

public sealed class StationAiOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> CameraStaticShader = "CameraStatic";
    private static readonly ProtoId<ShaderPrototype> StencilMaskShader = "StencilMask";
    private static readonly ProtoId<ShaderPrototype> StencilDrawShader = "StencilDraw";

    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly Robust.Shared.Configuration.IConfigurationManager _cfg = default!; // Goobstation - MalfAI camera upgrade

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly HashSet<Vector2i> _visibleTiles = new();

    private IRenderTexture? _staticTexture;
    private IRenderTexture? _stencilTexture;

    private float _updateRate = 1f / 30f;
    private float _accumulator;

    private EntityUid _lastGridUid = EntityUid.Invalid; // goobstation - off grid vision fix

    // Goobstation - MalfAI camera upgrade: reusable buffers for x-ray circles around cameras.
    private readonly List<Vector2> _circleCenters = new(16);
    private readonly List<(Vector2 pos, float dist2)> _cameraCandidates = new(32);

    public StationAiOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_stencilTexture?.Texture.Size != args.Viewport.Size)
        {
            _staticTexture?.Dispose();
            _stencilTexture?.Dispose();
            _stencilTexture = _clyde.CreateRenderTarget(args.Viewport.Size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "station-ai-stencil");
            _staticTexture = _clyde.CreateRenderTarget(args.Viewport.Size,
                new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
                name: "station-ai-static");
        }

        var worldHandle = args.WorldHandle;

        var worldBounds = args.WorldBounds;

        var playerEnt = _player.LocalEntity;

        // Shitmed - Starlight Abductors Change Start
        if (_entManager.TryGetComponent(playerEnt, out StationAiOverlayComponent? stationAiOverlay)
            && stationAiOverlay.AllowCrossGrid
            && _entManager.TryGetComponent(playerEnt, out RelayInputMoverComponent? relay))
            playerEnt = relay.RelayEntity;
        // Shitmed Change End

        _entManager.TryGetComponent(playerEnt, out TransformComponent? playerXform);
        var gridUid = playerXform?.GridUid ?? EntityUid.Invalid;
        _entManager.TryGetComponent(gridUid, out MapGridComponent? grid);
        _entManager.TryGetComponent(gridUid, out BroadphaseComponent? broadphase);

        // begin goobstation - off grid vision fix
        // If our current entity isn't on a valid grid/broadphase, reuse the last known valid grid so vision doesn't go black.
        if ((grid == null || broadphase == null) && _lastGridUid != EntityUid.Invalid)
        {
            if (_entManager.TryGetComponent(_lastGridUid, out MapGridComponent? lastGrid)
                && _entManager.TryGetComponent(_lastGridUid, out BroadphaseComponent? lastBroadphase))
            {
                grid = lastGrid;
                broadphase = lastBroadphase;
                gridUid = _lastGridUid;
            }
        }
        // end goobstation - off grid vision fix

        var invMatrix = args.Viewport.GetWorldToLocalMatrix();
        _accumulator -= (float) _timing.FrameTime.TotalSeconds;

        if (grid != null && broadphase != null)
        {
            _lastGridUid = gridUid; // goobstation - off grid vision fix

            var lookups = _entManager.System<EntityLookupSystem>();
            var xforms = _entManager.System<SharedTransformSystem>();

            if (_accumulator <= 0f)
            {
                _accumulator = MathF.Max(0f, _accumulator + _updateRate);
                _visibleTiles.Clear();
                _entManager.System<StationAiVisionSystem>().GetView((gridUid, broadphase, grid), worldBounds, _visibleTiles);
            }

            // Goobstation - MalfAI camera upgrade: collect x-ray circles around powered cameras
            // when the local player is a Malf AI with the upgrade active.
            _circleCenters.Clear();
            var radiusTiles = _cfg.GetCVar(CCVars.MalfAiCameraUpgradeRange);
            if (_entManager.TryGetComponent<MalfAiCameraUpgradeComponent>(_player.LocalEntity, out var camUpg)
                && camUpg.EnabledEffective
                && playerXform != null)
            {
                var appearance = _entManager.System<SharedAppearanceSystem>();
                var radiusWorld = radiusTiles * grid.TileSize;
                var worldAABB = worldBounds.CalcBoundingBox();
                var expanded = new Box2(worldAABB.Left - radiusWorld, worldAABB.Bottom - radiusWorld,
                    worldAABB.Right + radiusWorld, worldAABB.Top + radiusWorld);

                var eyeWorldPos = xforms.GetWorldPosition(playerEnt!.Value);
                _cameraCandidates.Clear();

                foreach (var camUid in lookups.GetEntitiesIntersecting(playerXform.MapID, expanded))
                {
                    if (_entManager.HasComponent<CameraUpgradeOmitterComponent>(camUid))
                        continue;

                    if (!_entManager.HasComponent<Content.Client.SurveillanceCamera.SurveillanceCameraVisualsComponent>(camUid)
                        || !_entManager.TryGetComponent(camUid, out TransformComponent? camXform))
                        continue;

                    // Only powered/active cameras project x-ray vision.
                    if (!_entManager.TryGetComponent(camUid, out AppearanceComponent? appearanceComp)
                        || !appearance.TryGetData(camUid, SurveillanceCameraVisualsKey.Key, out SurveillanceCameraVisuals state, appearanceComp))
                        continue;

                    if (state != SurveillanceCameraVisuals.Active && state != SurveillanceCameraVisuals.InUse)
                        continue;

                    var camPos = xforms.GetWorldPosition(camXform);
                    if (!expanded.Contains(camPos))
                        continue;

                    var d2 = (camPos - eyeWorldPos).LengthSquared();
                    _cameraCandidates.Add((camPos, d2));
                }

                // Closest cameras first, capped to keep the overlay cheap.
                _cameraCandidates.Sort((a, b) => a.dist2.CompareTo(b.dist2));
                var max = Math.Min(10, _cameraCandidates.Count);
                for (var i = 0; i < max; i++)
                    _circleCenters.Add(_cameraCandidates[i].pos);
            }
            // End Goobstation - MalfAI camera upgrade

            var gridMatrix = xforms.GetWorldMatrix(gridUid);
            var matty =  Matrix3x2.Multiply(gridMatrix, invMatrix);

            // Draw visible tiles to stencil
            worldHandle.RenderInRenderTarget(_stencilTexture!, () =>
            {
                worldHandle.SetTransform(matty);

                foreach (var tile in _visibleTiles)
                {
                    var aabb = lookups.GetLocalBounds(tile, grid.TileSize);
                    worldHandle.DrawRect(aabb, Color.White);
                }

                // Goobstation - MalfAI camera upgrade: union hard-edged discs around cameras.
                if (_circleCenters.Count > 0)
                {
                    worldHandle.SetTransform(invMatrix);
                    var circleRadius = radiusTiles * grid.TileSize;
                    foreach (var center in _circleCenters)
                    {
                        worldHandle.DrawCircle(center, circleRadius, Color.White, filled: true);
                    }
                }
                // End Goobstation - MalfAI camera upgrade
            },
            Color.Transparent);

            // Once this is gucci optimise rendering.
            worldHandle.RenderInRenderTarget(_staticTexture!,
            () =>
            {
                worldHandle.SetTransform(invMatrix);
                var shader = _proto.Index(CameraStaticShader).Instance();
                worldHandle.UseShader(shader);
                worldHandle.DrawRect(worldBounds, Color.White);
            },
            Color.Black);
        }
        // Not on a grid
        else
        {
            worldHandle.RenderInRenderTarget(_stencilTexture!, () =>
            {
            },
            Color.Transparent);

            worldHandle.RenderInRenderTarget(_staticTexture!,
            () =>
            {
                worldHandle.SetTransform(Matrix3x2.Identity);
                worldHandle.DrawRect(worldBounds, Color.Black);
            }, Color.Black);
        }

        // Use the lighting as a mask
        worldHandle.UseShader(_proto.Index(StencilMaskShader).Instance());
        worldHandle.DrawTextureRect(_stencilTexture!.Texture, worldBounds);

        // Draw the static
        worldHandle.UseShader(_proto.Index(StencilDrawShader).Instance());
        worldHandle.DrawTextureRect(_staticTexture!.Texture, worldBounds);

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(null);

    }
}
