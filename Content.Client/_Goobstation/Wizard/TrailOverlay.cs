using System.Numerics;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Goobstation.Wizard;

public sealed class TrailOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly IEntityManager _entManager;
    private readonly IPrototypeManager _protoMan;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    public TrailOverlay(IEntityManager entManager, IPrototypeManager protoMan)
    {
        ZIndex = (int) DrawDepth.Effects;

        _entManager = entManager;
        _protoMan = protoMan;
        _sprite = _entManager.System<SpriteSystem>();
        _transform = _entManager.System<TransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var bounds = args.WorldAABB.Enlarged(5f);

        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        var query = _entManager.EntityQueryEnumerator<TrailComponent, TransformComponent>();
        while (query.MoveNext(out _, out var trail, out var xform))
        {
            if (trail.TrailData.Count == 0)
                continue;

            if (trail.Shader != null && _protoMan.TryIndex<ShaderPrototype>(trail.Shader, out var shader))
                handle.UseShader(shader.Instance());
            else
                handle.UseShader(null);

            if (trail.Sprite == null)
            {
                handle.SetTransform(Matrix3x2.Identity);
                if (xform.MapID != MapId.Nullspace)
                {
                    var end = _transform.GetWorldPosition(xform, xformQuery);
                    var start = trail.TrailData[^1].Position;
                    DrawTrailLine(start, end, trail.Color, trail.LineThickness, bounds, handle);
                }
                for (var i = 1; i < trail.TrailData.Count; i++)
                {
                    var data = trail.TrailData[i];
                    var prevData = trail.TrailData[i - 1];

                    DrawTrailLine(prevData.Position, data.Position, data.Color, data.Thickness, bounds, handle);
                }

                continue;
            }

            var texture = _sprite.Frame0(trail.Sprite);
            var pos = -(Vector2) texture.Size / 2f / EyeManager.PixelsPerMeter;
            foreach (var data in trail.TrailData)
            {
                if (data.Color.A <= 0.01f)
                    continue;

                var worldPosition = data.Position;
                if (!bounds.Contains(worldPosition))
                    continue;

                handle.SetTransform(Matrix3Helpers.CreateTranslation(worldPosition));
                handle.DrawTexture(texture, pos, data.Angle, data.Color);
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    private void DrawTrailLine(Vector2 start,
        Vector2 end,
        Color color,
        float thickness,
        Box2 bounds,
        DrawingHandleWorld handle)
    {
        if (color.A <= 0.01f || thickness <= 0.01f)
            return;

        if (!bounds.Contains(start) || !bounds.Contains(end))
            return;

        var halfThickness = thickness * 0.5f;
        var direction = end - start;
        var angle = direction.ToAngle();
        var box = new Box2(start - new Vector2(0f, halfThickness),
            start + new Vector2(direction.Length(), halfThickness));
        var boxRotated = new Box2Rotated(box, angle, start);
        handle.DrawRect(boxRotated, color);
    }
}
