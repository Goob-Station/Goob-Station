using System.Numerics;
using Content.Shared._Shitcode.Heretic.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Shitcode.Heretic.Beam;

public sealed class ContinuousBeamOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly IEntityManager _entManager;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly IGameTiming _timing;

    private readonly ShaderInstance _unshadedShader;

    public ContinuousBeamOverlay(IEntityManager entManager, IPrototypeManager prototype, IGameTiming timing)
    {
        ZIndex = (int) Shared.DrawDepth.DrawDepth.Effects;

        _entManager = entManager;

        _timing = timing;

        _sprite = entManager.System<SpriteSystem>();
        _transform = entManager.System<TransformSystem>();

        _unshadedShader = prototype.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
        var query = _entManager.EntityQueryEnumerator<ContinuousBeamComponent, TransformComponent>();
        handle.UseShader(_unshadedShader);
        while (query.MoveNext(out var uid, out var beam, out var xform))
        {
            var coords = _transform.GetMapCoordinates(uid, xform);

            foreach (var (netTarget, data) in beam.Data)
            {
                if (!_entManager.TryGetEntity(netTarget, out var target) ||
                    !xformQuery.TryComp(target.Value, out var targetXforn))
                    continue;

                var targetCoords = _transform.GetMapCoordinates(target.Value, targetXforn);

                if (targetCoords.MapId != coords.MapId)
                    continue;

                var dir = targetCoords.Position - coords.Position;
                var length = dir.Length();
                var texture = _sprite.GetFrame(data.Sprite, _timing.CurTime);
                var textureSize = texture.Size;
                var realY = textureSize.Y / EyeManager.PixelsPerMeter;
                var realX = textureSize.X / EyeManager.PixelsPerMeter;
                var segments = (int) MathF.Ceiling(length / realY);
                if (segments <= 0)
                    continue;

                var ratio = length / (segments * realY);
                var angle = dir.ToWorldAngle();
                var normalized = dir.Normalized();
                var dir2 = new Vector2(normalized.X * realX / 2f, normalized.Y * realY / 2f);
                var pos = new Vector2(-realX / 2f, -realY / 2f) + dir2;
                var modifiedY = realY * ratio;

                handle.SetTransform(Matrix3Helpers.CreateTranslation(coords.Position));
                for (var i = 0; i < segments; i++)
                {
                    var quad = Box2.FromDimensions(pos + normalized * modifiedY * i, new Vector2(realX, modifiedY));
                    var quadRotated = new Box2Rotated(quad, angle, quad.Center);
                    handle.DrawTextureRect(texture, quadRotated, data.Color);
                }
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
