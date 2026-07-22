// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Graphics;
using Content.Shared.Power.EntitySystems; // goobstation - AI
using Content.Shared.Silicons.StationAi;
using Content.Shared.Wall; // goobstation - AI machine view
using Robust.Client.GameObjects; // goobstation - AI machine view
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map; // goobstation - AI machine view
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

    private static readonly ProtoId<ShaderPrototype> AiMachineViewShader = "AiMachineView"; // goobstation - AI machine view

    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly HashSet<Vector2i> _visibleTiles = new();
    private readonly HashSet<Entity<StationAiWhitelistComponent>> _aiMachineViewCandidates = new(); // goobstation - AI machine view

    private readonly OverlayResourceCache<CachedResources> _resources = new();

    private float _updateRate = 1f / 30f;
    private float _accumulator;

    private EntityUid _lastGridUid = EntityUid.Invalid; // goobstation - off grid vision fix

    public StationAiOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var res = _resources.GetForViewport(args.Viewport, static _ => new CachedResources());

        if (res.StencilTexture?.Texture.Size != args.Viewport.Size)
        {
            res.StaticTexture?.Dispose();
            res.StencilTexture?.Dispose();
            res.StencilTexture = _clyde.CreateRenderTarget(args.Viewport.Size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "station-ai-stencil");
            res.StaticTexture = _clyde.CreateRenderTarget(args.Viewport.Size,
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

            var gridMatrix = xforms.GetWorldMatrix(gridUid);
            var matty =  Matrix3x2.Multiply(gridMatrix, invMatrix);

            // Draw visible tiles to stencil
            worldHandle.RenderInRenderTarget(res.StencilTexture!, () =>
            {
                worldHandle.SetTransform(matty);

                foreach (var tile in _visibleTiles)
                {
                    var aabb = lookups.GetLocalBounds(tile, grid.TileSize);
                    worldHandle.DrawRect(aabb, Color.White);
                }
            },
            Color.Transparent);

            // Once this is gucci optimise rendering.
            worldHandle.RenderInRenderTarget(res.StaticTexture!,
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
            worldHandle.RenderInRenderTarget(res.StencilTexture!, () =>
            {
            },
            Color.Transparent);

            worldHandle.RenderInRenderTarget(res.StaticTexture!,
            () =>
            {
                worldHandle.SetTransform(Matrix3x2.Identity);
                worldHandle.DrawRect(worldBounds, Color.Black);
            }, Color.Black);
        }

        // Use the lighting as a mask
        worldHandle.UseShader(_proto.Index(StencilMaskShader).Instance());
        worldHandle.DrawTextureRect(res.StencilTexture!.Texture, worldBounds);

        // Draw the static
        worldHandle.UseShader(_proto.Index(StencilDrawShader).Instance());
        worldHandle.DrawTextureRect(res.StaticTexture!.Texture, worldBounds);

        // goobstation - AI machine view
        if (grid != null && broadphase != null)
            DrawAiMachineView(in args, worldHandle, gridUid, grid);

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(null);

    }

    // goobstation - AI machine view
    /// <summary>
    /// Renders with shader the sprite of every anchored, powered, AI-whitelisted machine that is hidden by the static.
    /// </summary>
    private void DrawAiMachineView(in OverlayDrawArgs args, DrawingHandleWorld worldHandle, EntityUid gridUid, MapGridComponent grid)
    {
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var lookup = _entManager.System<EntityLookupSystem>();
        var power = _entManager.System<SharedPowerReceiverSystem>();
        var sprites = _entManager.System<SpriteSystem>();
        var xforms = _entManager.System<SharedTransformSystem>();
        var maps = _entManager.System<SharedMapSystem>();

        _aiMachineViewCandidates.Clear();
        lookup.GetEntitiesIntersecting(args.MapId, args.WorldBounds, _aiMachineViewCandidates, LookupFlags.Static | LookupFlags.Sundries | LookupFlags.Approximate);

        var shader = _proto.Index(AiMachineViewShader).Instance();

        foreach (var ent in _aiMachineViewCandidates)
        {
            if (!ent.Comp.Enabled)
                continue;

            if (!_entManager.TryGetComponent<TransformComponent>(ent.Owner, out var entXform)
                || (!entXform.Anchored && !_entManager.HasComponent<WallMountComponent>(ent.Owner)) // WallMount fixtures (e.g. signal switches) aren't Anchored
                || entXform.GridUid != gridUid)
                continue;

            if (!power.IsPowered(ent.Owner))
                continue;


            var tile = maps.LocalToTile(gridUid, grid, entXform.Coordinates);
            if (_visibleTiles.Contains(tile))
                continue; //don't draw already visible

            if (!_entManager.TryGetComponent<SpriteComponent>(ent.Owner, out var sprite) || !sprite.Visible)
                continue;

            var (worldPos, worldRot) = xforms.GetWorldPositionRotation(entXform);

            worldHandle.UseShader(shader);
            sprites.RenderSprite((ent.Owner, sprite), worldHandle, eye.Rotation, worldRot, worldPos);
        }
    }
    // end goobstation - AI machine view

    protected override void DisposeBehavior()
    {
        _resources.Dispose();

        base.DisposeBehavior();
    }

    private sealed class CachedResources : IDisposable
    {
        public IRenderTexture? StaticTexture;
        public IRenderTexture? StencilTexture;

        public void Dispose()
        {
            StaticTexture?.Dispose();
            StencilTexture?.Dispose();
        }
    }
}
