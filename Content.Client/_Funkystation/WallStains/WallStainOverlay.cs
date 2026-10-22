using System.Linq;
using System.Numerics;
using Content.Client.Graphics;
using Content.Client.Light;
using Content.Shared._Funkystation.WallStains.Components;
using Content.Shared.Tag;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Funkystation.WallStains;

public sealed partial class WallStainOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";
    private static readonly ProtoId<ShaderPrototype> StencilMaskShader = "StencilMask";
    private static readonly ProtoId<ShaderPrototype> StencilEqualDrawShader = "StencilEqualDraw";

    private static readonly ProtoId<TagPrototype> DirectionalWindowTag = "DirectionalWindow";
    private static readonly ProtoId<TagPrototype> WindowTag = "Window";
    private static readonly ProtoId<TagPrototype> AirlockTag = "Airlock";
    private static readonly ProtoId<TagPrototype> WallTag = "Wall";

    [Dependency] private IClyde _clyde = null!;
    [Dependency] private IEntityManager _entityManager = null!;
    [Dependency] private IPrototypeManager _prototypeManager = null!;
    [Dependency] private IGameTiming _gameTiming = null!;

    private readonly TransformSystem _transformSystem;
    private readonly SpriteSystem _spriteSystem;
    private readonly EntityLookupSystem _entityLookupSystem;
    private readonly TagSystem _tagSystem;

    private readonly EntityQuery<TransformComponent> _transformQuery;

    private TimeSpan _lastLayoutPrune = TimeSpan.Zero;
    private static readonly TimeSpan LayoutPruneInterval = TimeSpan.FromSeconds(30);

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private readonly HashSet<Entity<WallStainComponent>> _visibleStains = [];
    private readonly HashSet<EntityUid> _tempEntities = [];
    private readonly HashSet<EntityUid> _intersectingEntities = [];
    private readonly OverlayResourceCache<CachedResources> _resources = new();

    private readonly Dictionary<EntityUid, SplatLayout> _splatLayouts = new();

    private readonly Dictionary<EntityUid, StencilCacheEntry> _stencilCache = new();
    private static readonly TimeSpan StencilCacheLifetime = TimeSpan.FromSeconds(2);

    private sealed class StencilCacheEntry
    {
        public TimeSpan ComputedAt;
        public readonly List<EntityUid> Entities = new();
    }

    private const float DblPixelsPerMeter = 2f * EyeManager.PixelsPerMeter;

    // min/max splat instances a stain can draw, scaled by how full it is
    private const int MinSplats = 1;
    private const int MaxSplats = 5;

    // how far individual splat instances can stray from the stain's origin
    private const float SplatSpread = 0.5f;

    private readonly record struct SplatInstance(Vector2 Offset);

    private sealed class SplatLayout
    {
        public int Seed;
        public float FillLevel;
        public SplatInstance[] Instances = Array.Empty<SplatInstance>();
    }

    public WallStainOverlay()
    {
        IoCManager.InjectDependencies(this);

        _transformSystem = _entityManager.System<TransformSystem>();
        _spriteSystem = _entityManager.System<SpriteSystem>();
        _entityLookupSystem = _entityManager.System<EntityLookupSystem>();
        _tagSystem = _entityManager.System<TagSystem>();

        _transformQuery = _entityManager.GetEntityQuery<TransformComponent>();

        ZIndex = AfterLightTargetOverlay.ContentZIndex + 1;
    }

    private SplatLayout GetSplatLayout(Entity<WallStainComponent> stainEntity)
    {
        var stain = stainEntity.Comp;

        if (_splatLayouts.TryGetValue(stainEntity.Owner, out var cached) &&
            cached.Seed == stain.SplatSeed &&
            MathF.Abs(cached.FillLevel - stain.FillLevel) < 0.05f)
        {
            return cached;
        }

        var count = Math.Clamp(MinSplats + (int) MathF.Round(stain.FillLevel * (MaxSplats - MinSplats)), MinSplats, MaxSplats);
        var rand = new Random(stain.SplatSeed);

        var instances = new SplatInstance[count];
        for (var i = 0; i < count; i++)
        {
            var offset = new Vector2(
                (float) (rand.NextDouble() - 0.5) * SplatSpread,
                (float) (rand.NextDouble() - 0.5) * SplatSpread);
            instances[i] = new SplatInstance(offset);
        }

        var layout = new SplatLayout
        {
            Seed = stain.SplatSeed,
            FillLevel = stain.FillLevel,
            Instances = instances
        };

        _splatLayouts[stainEntity.Owner] = layout;
        return layout;
    }

    private StencilCacheEntry ComputeStencilTargets(EntityUid stainUid, TransformComponent stainXform, MapId mapId, TimeSpan realTime)
    {
        var entry = _stencilCache.TryGetValue(stainUid, out var existing) ? existing : new StencilCacheEntry();
        entry.Entities.Clear();
        entry.ComputedAt = realTime;

        var stainWorldPos = _transformSystem.GetWorldPosition(stainXform);
        var queryBox = Box2.CenteredAround(stainWorldPos, new Vector2(3.0f, 3.0f));

        _tempEntities.Clear();
        _entityLookupSystem.GetEntitiesIntersecting(mapId, queryBox, _tempEntities, LookupFlags.Static);

        foreach (var uid in _tempEntities)
        {
            // We only want to draw stencil masks on entities that have a Sprite and Transform
            if (!_transformQuery.TryGetComponent(uid, out var transformComponent) ||
                !_entityManager.TryGetComponent<SpriteComponent>(uid, out _))
                continue;

            // Andddd only draw stains onto anchored entities
            if (!transformComponent.Anchored)
                continue;

            // AAAAAAAAAAND directional windows don't cover the full tile, so skip them to avoid floating stains
            if (_tagSystem.HasTag(uid, DirectionalWindowTag))
                continue;

            // Finally, make sure the entity is one of the following:
            if (!_tagSystem.HasTag(uid, WallTag) &&
                !_tagSystem.HasTag(uid, WindowTag) &&
                !_tagSystem.HasTag(uid, AirlockTag))
            {
                continue;
            }

            entry.Entities.Add(uid);
        }

        _stencilCache[stainUid] = entry;
        return entry;
    }

    private void PruneStaleCaches()
    {
        if (_splatLayouts.Count == 0 && _stencilCache.Count == 0)
            return;

        List<EntityUid>? toRemove = null;
        foreach (var uid in _splatLayouts.Keys.Concat(_stencilCache.Keys))
        {
            if (_entityManager.EntityExists(uid))
                continue;

            toRemove ??= new List<EntityUid>();
            if (!toRemove.Contains(uid))
                toRemove.Add(uid);
        }

        if (toRemove == null)
            return;

        foreach (var uid in toRemove)
        {
            _splatLayouts.Remove(uid);
            _stencilCache.Remove(uid);
        }
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = args.Viewport;
        var mapId = args.MapId;
        var worldBounds = args.WorldBounds;
        var worldHandle = args.WorldHandle;
        var target = viewport.RenderTarget;
        var invMatrix = viewport.GetWorldToLocalMatrix();
        var realTime = _gameTiming.RealTime;

        _visibleStains.Clear();
        _entityLookupSystem.GetEntitiesIntersecting(mapId, worldBounds, _visibleStains);

        if (_visibleStains.Count == 0)
            return;

        if (realTime - _lastLayoutPrune > LayoutPruneInterval)
        {
            PruneStaleCaches();
            _lastLayoutPrune = realTime;
        }

        var res = _resources.GetForViewport(viewport, static _ => new CachedResources());

        if (res.StainTarget?.Texture.Size != target.Size)
        {
            res.StainTarget?.Dispose();
            res.StainTarget = _clyde.CreateRenderTarget(target.Size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "stain-stencil-target");
        }

        args.WorldHandle.RenderInRenderTarget(res.StainTarget,
            () =>
            {
                _intersectingEntities.Clear();

                worldHandle.UseShader(_prototypeManager.Index(UnshadedShader).Instance());

                foreach (var stainEntity in _visibleStains)
                {
                    if (!_transformQuery.TryGetComponent(stainEntity.Owner, out var stainXform))
                        continue;

                    if (!_stencilCache.TryGetValue(stainEntity.Owner, out var cacheEntry) ||
                        realTime - cacheEntry.ComputedAt > StencilCacheLifetime)
                    {
                        cacheEntry = ComputeStencilTargets(stainEntity.Owner, stainXform, mapId, realTime);
                    }

                    foreach (var wallUid in cacheEntry.Entities)
                    {
                        _intersectingEntities.Add(wallUid);
                    }
                }

                foreach (var uid in _intersectingEntities)
                {
                    if (!_transformQuery.TryGetComponent(uid, out var transformComponent) ||
                        !_entityManager.TryGetComponent<SpriteComponent>(uid, out var spriteComponent))
                        continue;

                    if (transformComponent.GridUid == null)
                        continue;

                    var gridUid = transformComponent.GridUid.Value;
                    var localMatrix = Matrix3x2.Multiply(_transformSystem.GetWorldMatrix(gridUid, _transformQuery), invMatrix);
                    worldHandle.SetTransform(localMatrix);

                    var bounds = _spriteSystem.CalculateBounds((uid, spriteComponent), transformComponent.Coordinates.Position, transformComponent.LocalRotation, viewport.Eye?.Rotation ?? Angle.Zero);
                    worldHandle.DrawRect(bounds, Color.White);
                }

            },
            Color.Transparent);

        worldHandle.SetTransform(Matrix3x2.Identity);

        worldHandle.UseShader(_prototypeManager.Index(StencilMaskShader).Instance());
        worldHandle.DrawTextureRect(res.StainTarget.Texture, worldBounds);

        worldHandle.UseShader(_prototypeManager.Index(StencilEqualDrawShader).Instance());

        foreach (var stainEntity in _visibleStains)
        {
            var uid = stainEntity.Owner;
            var stain = stainEntity.Comp;

            if (!_transformQuery.TryGetComponent(uid, out var xform))
                continue;

            var state = string.IsNullOrEmpty(stain.StainState) ? "splatter" : stain.StainState;
            var rsiSpec = new SpriteSpecifier.Rsi(new ResPath("/Textures/Effects/crayondecals.rsi"), state);

            Texture? texture;
            try
            {
                texture = _spriteSystem.GetFrame(rsiSpec, realTime);
            }
            catch (Exception)
            {
                try
                {
                    var fallbackSpec = new SpriteSpecifier.Rsi(new ResPath("/Textures/Effects/crayondecals.rsi"), "splatter");
                    texture = _spriteSystem.GetFrame(fallbackSpec, realTime);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            var halfWidth = texture.Width / DblPixelsPerMeter;
            var halfHeight = texture.Height / DblPixelsPerMeter;
            var rect = new Box2(-halfWidth, -halfHeight, halfWidth, halfHeight);

            var (_, _, worldMatrix) = _transformSystem.GetWorldPositionRotationMatrix(xform);

            // scatter a bunch of decals around instead of stretching one decal
            var layout = GetSplatLayout(stainEntity);

            foreach (var instance in layout.Instances)
            {
                var instanceMatrix = Matrix3x2.CreateTranslation(instance.Offset);
                worldHandle.SetTransform(Matrix3x2.Multiply(instanceMatrix, worldMatrix));

                worldHandle.DrawTextureRect(
                    texture,
                    rect,
                    modulate: stain.Color
                );
            }
        }

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(null);
    }

    protected override void DisposeBehavior()
    {
        _resources.Dispose();
        base.DisposeBehavior();
    }

    private sealed class CachedResources : IDisposable
    {
        public IRenderTexture? StainTarget;

        public void Dispose()
        {
            StainTarget?.Dispose();
        }
    }
}
