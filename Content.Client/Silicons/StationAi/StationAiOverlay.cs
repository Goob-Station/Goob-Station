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
using Content.Shared.Silicons.StationAi;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Log;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Movement.Components; // Shitmed - Starlight Abductors Change
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Content.Shared._Funkystation.CCVar;
using Content.Shared._Funkystation.MalfAI.Components;

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
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly ISawmill _sawmill = Logger.GetSawmill("station-ai.overlay");

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly HashSet<Vector2i> _visibleTiles = new();

    private IRenderTexture? _staticTexture;
    private IRenderTexture? _stencilTexture;

    private float _updateRate = 1f / 30f;
    private float _accumulator;

    // Cached systems and resources for performance
    private SharedAppearanceSystem? _appearance; // Funkystation -> Malf AI
    private EntityLookupSystem? _lookup; // Funkystation -> Malf AI

    // Reusable buffers to avoid per-frame allocations
    private readonly List<Vector2> _circleCenters = new(16); // Funkystation -> Malf AI
    private readonly List<(Vector2 pos, float dist2)> _cameraCandidates = new(32); // Funkystation -> Malf AI

    private EntityUid _lastGridUid = EntityUid.Invalid; // goobstation - off grid vision fix

    public StationAiOverlay()
    {
        IoCManager.InjectDependencies(this);

        // Cache systems
        _lookup = _entManager.System<EntityLookupSystem>(); // Funkystation -> Malf AI
        _appearance = _entManager.System<SharedAppearanceSystem>(); // Funkystation -> Malf AI
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
        var worldAABB = worldBounds.CalcBoundingBox(); // Funkystation -> Malf AI

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

            // Funkystation -> Malf AI. Determine whether the Malf camera-upgrade is active for this client.
            var malfActive = false;
            var radiusTiles = _cfg.GetCVar(CCVarsMalfAi.MalfAiCameraUpgradeRange);

            if (_entManager.TryGetComponent<MalfAiCameraUpgradeComponent>(playerEnt, out var camUpg))
                malfActive = camUpg.EnabledEffective;


            if (_accumulator <= 0f)
            {
                _accumulator = MathF.Max(0f, _accumulator + _updateRate);
                _visibleTiles.Clear();

                // Funkystation -> Malf AI. Always include LOS-based station AI vision tiles.
                _entManager.System<StationAiVisionSystem>().GetView((gridUid, broadphase, grid), worldBounds, _visibleTiles);
                // Funkystation -> Malf AI. Do NOT dilate tiles when the camera upgrade is active; circles will be unioned instead.
            }

            // Funkystation -> Malf AI begin. Compute circle centers (eligible cameras) if upgrade is active
            _circleCenters.Clear();
            if (malfActive)
            {
                var xformSys = xforms;

                // Radius in world units (tiles -> world)
                var radiusWorld = radiusTiles * (grid?.TileSize ?? 1f);

                // Expand the world AABB by the circle radius to include near-offscreen cameras
                var expanded = new Box2(worldAABB.Left - radiusWorld, worldAABB.Bottom - radiusWorld,
                    worldAABB.Right + radiusWorld, worldAABB.Top + radiusWorld);

                // Eye/world position for distance sorting
                var eyeWorldPos = xformSys.GetWorldPosition(playerEnt!.Value);

                _cameraCandidates.Clear();

                // Use spatial lookup to avoid scanning all entities
                if (playerXform != null && _lookup != null)
                {
                    foreach (var camUid in _lookup.GetEntitiesIntersecting(playerXform.MapID, expanded))
                    {
                        // Skip entities marked to be omitted from camera upgrades
                        if (_entManager.HasComponent<CameraUpgradeOmitterComponent>(camUid))
                            continue;

                        // Only consider entities with surveillance camera visuals component
                        if (!_entManager.HasComponent<Content.Client.SurveillanceCamera.SurveillanceCameraVisualsComponent>(camUid) ||
                            !_entManager.TryGetComponent(camUid, out TransformComponent? camXform))
                            continue;

                        // Check if camera is powered/active via appearance data
                        if (!_entManager.TryGetComponent(camUid, out AppearanceComponent? appearance) ||
                            _appearance == null ||
                            !_appearance.TryGetData(camUid, Content.Shared.SurveillanceCamera.SurveillanceCameraVisualsKey.Key,
                                out Content.Shared.SurveillanceCamera.SurveillanceCameraVisuals state, appearance))
                            continue;

                        if (state != Content.Shared.SurveillanceCamera.SurveillanceCameraVisuals.Active &&
                            state != Content.Shared.SurveillanceCamera.SurveillanceCameraVisuals.InUse)
                            continue;

                        var camPos = xformSys.GetWorldPosition(camXform);
                        // Within expanded viewport bounds (redundant but cheap safety)
                        if (!expanded.Contains(camPos))
                            continue;

                        var d2 = (camPos - eyeWorldPos).LengthSquared();
                        _cameraCandidates.Add((camPos, d2));
                    }

                    // Sort by distance and take up to 10
                    _cameraCandidates.Sort((a, b) => a.dist2.CompareTo(b.dist2));
                    var max = Math.Min(10, _cameraCandidates.Count);
                    for (var i = 0; i < max; i++)
                        _circleCenters.Add(_cameraCandidates[i].pos);
                }
                // Funkystation -> Malf AI end.
            }

            var gridMatrix = xforms.GetWorldMatrix(gridUid);
            var matty = Matrix3x2.Multiply(gridMatrix, invMatrix);

            // Draw selected tiles to stencil and union circles
            worldHandle.RenderInRenderTarget(_stencilTexture!, () =>
            {
                // 1) Tiles in local-grid space
                worldHandle.SetTransform(matty);
                foreach (var tile in _visibleTiles)
                {
                    var aabb = lookups.GetLocalBounds(tile, grid!.TileSize);
                    worldHandle.DrawRect(aabb, Color.White);
                }

                // Funkystation -> Malf AI.
                // 2) Camera circles in world space (hard-edged filled discs)
                if (_circleCenters.Count > 0)
                {
                    worldHandle.SetTransform(invMatrix);
                    var radiusWorld = radiusTiles * (grid!.TileSize);
                    foreach (var center in _circleCenters)
                    {
                        worldHandle.DrawCircle(center, radiusWorld, Color.White, filled: true);
                    }
                }
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
        var stencilMaskShader = _proto.Index(StencilMaskShader).Instance();
        var stencilDrawShader = _proto.Index(StencilDrawShader).Instance();

        worldHandle.UseShader(stencilMaskShader); // Funkystation -> Malf AI.
        worldHandle.DrawTextureRect(_stencilTexture!.Texture, worldBounds);

        // Draw the static
        worldHandle.UseShader(stencilDrawShader);
        worldHandle.DrawTextureRect(_staticTexture!.Texture, worldBounds);

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(null);
    }

    // Funkystation -> Malf AI.
    // Expands the set of visible tiles by a given Chebyshev radius using 8-directional dilation.
    private static void ExpandVisibleTiles(HashSet<Vector2i> tiles, int extraRadius)
    {
        if (extraRadius <= 0 || tiles.Count == 0)
            return;

        // Work on a copy to avoid modifying during iteration
        var expanded = new HashSet<Vector2i>(tiles);

        // 8-directional offsets including diagonals
        var offsets = new Vector2i[]
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
        };

        for (var step = 0; step < extraRadius; step++)
        {
            // snapshot current frontier
            var snapshot = new System.Collections.Generic.List<Vector2i>(expanded);
            foreach (var t in snapshot)
            {
                foreach (var o in offsets)
                {
                    var n = t + o;
                    expanded.Add(n);
                }
            }
        }

        tiles.Clear();
        foreach (var t in expanded)
            tiles.Add(t);
    }
}
