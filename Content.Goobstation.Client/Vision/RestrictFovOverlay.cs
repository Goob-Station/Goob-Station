using System.Numerics;
using Content.Goobstation.Shared.Vision;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Containers;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Vision;

public sealed partial class RestrictFovOverlay : Overlay
{
    [Dependency] private IEntityManager _entity = default!;
    [Dependency] private IPlayerManager _player = default!;

    private SharedTransformSystem? _transform;
    private SharedContainerSystem? _container;
    [Dependency] private EntityQuery<RestrictFovComponent> _fovQuery;
    [Dependency] private EntityQuery<TransformComponent> _xformQuery;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => false;

    private const int AngleSteps = 240;
    private const int NearBands = 8;
    private const float Radius = 80f;

    private readonly List<DrawVertexUV2DColor> _vertices = new();

    public RestrictFovOverlay()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = 200;
    }

    private EntityUid GetAnchor(EntityUid uid)
    {
        while (_container!.TryGetContainingContainer((uid, null, null), out var cont))
            uid = cont.Owner;

        return uid;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        _transform ??= _entity.System<SharedTransformSystem>();
        _container ??= _entity.System<SharedContainerSystem>();

        if (!_fovQuery.TryComp(_player.LocalEntity, out var fov))
            return;

        var xform = _xformQuery.GetComponent(GetAnchor(_player.LocalEntity.Value));
        if (xform.MapID != args.MapId)
            return;

        var (center, worldRot) = _transform.GetWorldPositionRotation(xform);
        var facing = worldRot.ToWorldVec();
        var heading = MathF.Atan2(facing.Y, facing.X);

        var half = MathHelper.DegreesToRadians(fov.FovDegrees) * 0.5f;
        var feather = MathF.Max(0.001f, MathHelper.DegreesToRadians(fov.FeatherDegrees));
        var near = MathF.Max(0f, fov.NearUserPityRadius);
        var nearFeather = Math.Clamp(fov.NearFeather, 0f, near);
        var innerRadius = near - nearFeather;

        var radii = new float[NearBands + 2];
        for (var j = 0; j <= NearBands; j++)
            radii[j] = innerRadius + nearFeather * j / NearBands;

        radii[NearBands + 1] = Radius;

        var handle = args.WorldHandle;

        for (var j = 0; j < radii.Length - 1; j++)
        {
            var r0 = radii[j];
            var r1 = radii[j + 1];
            var g0 = GraceFactor(r0, innerRadius, near);
            var g1 = GraceFactor(r1, innerRadius, near);

            if (g1 <= 0f)
                continue;

            _vertices.Clear();
            for (var i = 0; i <= AngleSteps; i++)
            {
                var angle = MathF.Tau * i / AngleSteps;
                var cone = ConeFactor(angle, heading, half, feather);
                var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

                _vertices.Add(new DrawVertexUV2DColor(center + dir * r0, new Color(0f, 0f, 0f, fov.OutsideFovOpacity * cone * g0)));
                _vertices.Add(new DrawVertexUV2DColor(center + dir * r1, new Color(0f, 0f, 0f, fov.OutsideFovOpacity * cone * g1)));
            }

            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleStrip, Texture.White, _vertices.ToArray());
        }
    }

    private static float ConeFactor(float angle, float heading, float half, float feather)
    {
        var off = MathF.Abs((float) Angle.ShortestDistance(heading, angle).Theta);
        if (off <= half)
            return 0f;

        var t = Math.Clamp((off - half) / feather, 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private static float GraceFactor(float r, float innerRadius, float near)
    {
        if (near <= 0f)
            return 1f;

        if (r <= innerRadius)
            return 0f;

        if (r >= near)
            return 1f;

        var t = (r - innerRadius) / (near - innerRadius);
        return t * t * (3f - 2f * t);
    }
}
