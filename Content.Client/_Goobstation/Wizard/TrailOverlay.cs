using System.Numerics;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Goobstation.Wizard;

public sealed class TrailOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly IEntityManager _entManager;
    private readonly IPrototypeManager _protoMan;
    private readonly SpriteSystem _sprite;

    public TrailOverlay(IEntityManager entManager, IPrototypeManager protoMan)
    {
        ZIndex = (int) DrawDepth.Effects;

        _entManager = entManager;
        _protoMan = protoMan;
        _sprite = _entManager.System<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var bounds = args.WorldAABB.Enlarged(5f);

        var query = _entManager.EntityQueryEnumerator<TrailComponent>();
        while (query.MoveNext(out _, out var trail))
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
                for (var i = 1; i < trail.TrailData.Count; i++)
                {
                    var data = trail.TrailData[i];

                    if (!bounds.Contains(data.Position))
                        continue;

                    if (data.Color.A <= 0.01f || data.Thickness <= 0.01f)
                        continue;

                    var prevData = trail.TrailData[i - 1];

                    if (!bounds.Contains(prevData.Position))
                        continue;

                    var halfThickness = data.Thickness / 2f;
                    var direction = data.Position - prevData.Position;
                    var angle = direction.ToAngle();
                    var box = new Box2(prevData.Position - new Vector2(0f, halfThickness),
                        prevData.Position + new Vector2(direction.Length(), halfThickness));
                    var boxRotated = new Box2Rotated(box, angle, prevData.Position);
                    handle.DrawRect(boxRotated, data.Color);
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
}
