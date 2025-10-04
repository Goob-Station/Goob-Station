using System.Numerics;
using Content.Goobstation.Shared.Obsession;
using Content.Shared.Body.Components;
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Obsession;

public sealed class ObsessionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly SharedTransformSystem _transform;
    private readonly ContainerSystem _container;
    private readonly SpriteSystem _sprite;

    private readonly EntityQuery<ObsessionTargetComponent> _targetQuery;
    private readonly EntityQuery<SpriteComponent> _spriteQuery;
    private readonly EntityQuery<TransformComponent> _xformQuery;

    private readonly ShaderInstance _blurShaderX;
    private float _blurAmount = 0f;
    public int MaxBlur = 0;
    public int ObsessionId = 0;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public ObsessionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _blurShaderX = _proto.Index<ShaderPrototype>("BlurryVisionX").InstanceUnique();

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<SharedTransformSystem>();
        _sprite = _entity.System<SpriteSystem>();

        _targetQuery = _entity.GetEntityQuery<ObsessionTargetComponent>();
        _spriteQuery = _entity.GetEntityQuery<SpriteComponent>();
        _xformQuery = _entity.GetEntityQuery<TransformComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        if (_blurAmount <= 0)
            return;

        var eye = args.Viewport.Eye;
        if (eye == null)
            return;

        var mapId = eye.Position.MapId;
        var eyeRot = eye.Rotation;
        var worldBounds = args.WorldBounds;
        var worldHandle = args.WorldHandle;

        _blurShaderX.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _blurShaderX.SetParameter("BLUR_AMOUNT", _blurAmount * 0.1f);
        worldHandle.UseShader(_blurShaderX);
        worldHandle.DrawRect(args.WorldBounds, Color.White);
        worldHandle.UseShader(null);

        if (_blurAmount < 2)
            return;

        foreach (var entity in _entity.GetEntities())
        {
            if (!_targetQuery.TryGetComponent(entity, out var comp) || comp.Id != ObsessionId)
                continue;

            if (!_spriteQuery.TryGetComponent(entity, out var sprite) || sprite.DrawDepth < -3 || sprite.DrawDepth > 7)
                continue;

            if (!_xformQuery.TryGetComponent(entity, out var xform))
                continue;

            if (_container.IsEntityInContainer(entity))
                continue;

            if (xform.MapID != mapId)
                continue;

            var worldPos = _transform.GetWorldPosition(xform);

            if (!worldBounds.Enlarged(1.5f).Contains(worldPos))
                continue;

            DrawEntity((entity, sprite, xform), worldHandle, eyeRot);
        }
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_blurAmount < MaxBlur)
        {
            _blurAmount += args.DeltaSeconds * 0.05f;
            _blurAmount = Math.Clamp(_blurAmount, 0, MaxBlur);
        }
        else if (_blurAmount > MaxBlur)
        {
            _blurAmount -= args.DeltaSeconds * 0.15f;
            _blurAmount = Math.Clamp(_blurAmount, MaxBlur, 4);
        }
    }

    private void DrawEntity(
        Entity<SpriteComponent, TransformComponent> ent,
        DrawingHandleWorld handle,
        Angle eyeRot)
    {
        var (uid, sprite, xform) = ent;
        var position = _transform.GetWorldPosition(xform);
        var rotation = _transform.GetWorldRotation(xform);

        handle.SetTransform(position, rotation);

        _sprite.RenderSprite((uid, sprite), handle, eyeRot, rotation, position);

        handle.SetTransform(Vector2.Zero, Angle.Zero);
    }
}
