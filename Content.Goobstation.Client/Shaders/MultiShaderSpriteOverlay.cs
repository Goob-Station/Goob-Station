using System.Linq;
using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Shaders;

public sealed class MultiShaderSpriteOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    private readonly TransformSystem _transform;
    private readonly SpriteSystem _sprite;
    private readonly ContainerSystem _container;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private readonly Dictionary<EntityUid, IRenderTexture> _renderTargets = new();

    public MultiShaderSpriteOverlay()
    {
        IoCManager.InjectDependencies(this);

        _transform = _entMan.System<TransformSystem>();
        _sprite = _entMan.System<SpriteSystem>();
        _container = _entMan.System<ContainerSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = args.Viewport;

        if (viewport.Eye is not { } eye)
            return;

        var handle = args.WorldHandle;
        var bounds = args.WorldAABB.Enlarged(2f);

        var localMatrix = viewport.GetWorldToLocalMatrix();

        var processed = new List<EntityUid>();

        var query = _entMan.EntityQueryEnumerator<MultiShaderSpriteComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var multi, out var sprite, out var xform))
        {
            if (multi.PostShaders.Count == 0 || !sprite.Visible || _container.IsEntityInContainer(uid))
                continue;

            var (pos, rot) = _transform.GetWorldPositionRotation(xform);

            if (!bounds.Contains(pos))
                continue;

            var spriteBB = _sprite.CalculateBounds((uid, sprite), pos, rot, eye.Rotation);
            var screenBB = localMatrix.TransformBox(spriteBB.Box);
            var screenSpriteSize = (Vector2i) screenBB.Size.Rounded();

            if (screenSpriteSize.X == 0 || screenSpriteSize.Y == 0)
                continue;

            if (screenSpriteSize.X % 2 != 0)
                screenSpriteSize.X++;
            if (screenSpriteSize.Y % 2 != 0)
                screenSpriteSize.Y++;

            processed.Add(uid);

            if (!_renderTargets.TryGetValue(uid, out var target))
            {
                target = _clyde.CreateRenderTarget(screenSpriteSize,
                    new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb, true),
                    name: $"multi_shader_{uid}");

                _renderTargets[uid] = target;
            }
            else if (target.Size != screenSpriteSize)
            {
                target.Dispose();

                target = _clyde.CreateRenderTarget(screenSpriteSize,
                    new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb, true),
                    name: $"multi_shader_{uid}");

                _renderTargets[uid] = target;
            }

            var quad = Box2.FromDimensions(Vector2.Zero, screenSpriteSize).Scale(new Vector2(1f, -1f));

            var postHandle = new DrawingHandleMultiShader(Texture.White, handle);

            postHandle.RenderInRenderTarget(target,
                () =>
                {
                    var position = target.LocalToWorld(eye, (Vector2) screenSpriteSize * 0.5f, viewport.RenderScale);

                    var angle = rot + eye.Rotation;
                    angle = angle.Reduced().FlipPositive();

                    var cardinal = Angle.Zero;

                    if (sprite is {NoRotation: false, SnapCardinals: true})
                        cardinal = angle.RoundToCardinalAngle();

                    var entityMatrix = Matrix3Helpers.CreateTransform(position, sprite.NoRotation ? -eye.Rotation : rot - cardinal);
                    Matrix3x2.Invert(entityMatrix, out var invEntityMatrix);

                    var invMatrix = target.GetWorldToLocalMatrix(eye, viewport.RenderScale);

                    Matrix3x2.Invert(sprite.LocalMatrix, out var invSpriteMatrix);

                    var theta = (float) eye.Rotation.Theta;
                    var absSin = MathF.Abs(MathF.Sin(theta));
                    var absCos = MathF.Abs(MathF.Cos(theta));
                    var s = sprite.Scale;
                    var scale = new Vector2(absCos * s.X + absSin * s.Y, absSin * s.X + absCos * s.Y);

                    var scaleMatrix = Matrix3Helpers.CreateScale(scale);

                    postHandle.InvMatrix = invEntityMatrix * invSpriteMatrix * scaleMatrix * entityMatrix * invMatrix;

                    _sprite.RenderSprite((uid, sprite), postHandle, eye.Rotation, rot, position);
                    postHandle.InvMatrix = Matrix3x2.Identity;

                    postHandle.SetTransform(Matrix3x2.Identity);

                    foreach (var (protoId, data) in multi.PostShaders.OrderBy(x => x.Value.RenderOrder))
                    {
                        var proto = _proto.Index<ShaderPrototype>(protoId);
                        var shader = data.Mutable ? proto.Instance() : proto.InstanceUnique();
                        if (data.RaiseShaderEvent)
                        {
                            var ev = new BeforePostMultiShaderRenderEvent(proto, shader, sprite, viewport);
                            _entMan.EventBus.RaiseLocalEvent(uid, ref ev);
                        }

                        postHandle.UseShader(shader);
                        postHandle.DrawTextureRectRegion(target.Texture, quad, data.Color);
                    }

                    if (sprite.PostShader == null)
                        return;

                    postHandle.UseShader(sprite.PostShader);
                    if (sprite.RaiseShaderEvent)
                        _entMan.EventBus.RaiseLocalEvent(uid, new BeforePostShaderRenderEvent(sprite, viewport));
                    postHandle.DrawTextureRectRegion(target.Texture, quad);
                },
                Color.Transparent);

            handle.UseShader(null);
            handle.SetTransform(Matrix3x2.Identity);
            handle.DrawTextureRectRegion(target.Texture, spriteBB);
        }

        handle.SetTransform(Matrix3x2.Identity);

        foreach (var (key, target) in _renderTargets)
        {
            if (processed.Contains(key))
                continue;

            target.Dispose();
            _renderTargets.Remove(key);
        }
    }

    protected override void DisposeBehavior()
    {
        base.DisposeBehavior();

        foreach (var (_, target) in _renderTargets)
        {
            target.Dispose();
        }

        _renderTargets.Clear();
    }
}
