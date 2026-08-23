using System.Numerics;
using Content.Client.Parallax;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.ParallaxStargaze;

public sealed class ParallaxStargazeOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";

    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    private readonly ParallaxStargazeSystem _stargaze;
    private readonly ParallaxSystem _parallax;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _sprite;
    private readonly ShaderInstance _unshaded;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public ParallaxStargazeOverlay()
    {
        ZIndex = ParallaxSystem.ParallaxZIndex + 2;
        IoCManager.InjectDependencies(this);
        _stargaze = _entManager.System<ParallaxStargazeSystem>();
        _parallax = _entManager.System<ParallaxSystem>();
        _transform = _entManager.System<SharedTransformSystem>();
        _sprite = _entManager.System<SpriteSystem>();
        _unshaded = _protoManager.Index(UnshadedShader).Instance();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _stargaze.ActiveCampfire != null && _stargaze.Progress > 0f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_stargaze.Progress <= 0f)
            return;

        var handle = args.WorldHandle;
        var worldAABB = args.WorldAABB;
        var position = args.Viewport.Eye?.Position.Position ?? Vector2.Zero;
        var eyeRotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var curTime = _timing.RealTime;
        var alpha = _stargaze.Progress;

        handle.UseShader(_unshaded);
        foreach (var layer in _parallax.GetParallaxLayers(args.MapId))
        {
            _parallax.DrawParallax(
                handle,
                worldAABB,
                layer.Texture,
                curTime,
                position,
                layer.Config.Scrolling,
                layer.Config.Scale.X,
                layer.Config.Slowness,
                Color.White.WithAlpha(alpha));
        }

        handle.UseShader(null);
        foreach (var uid in _stargaze.Exempt)
        {
            if (!_entManager.TryGetComponent(uid, out SpriteComponent? sprite))
                continue;

            var worldPos = _transform.GetWorldPosition(uid);
            var worldRot = _transform.GetWorldRotation(uid);
            _sprite.RenderSprite((uid, sprite), handle, eyeRotation, worldRot, worldPos);
        }

        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(null);
    }
}
