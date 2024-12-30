using System.Numerics;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Goobstation.Wizard;

public sealed class TrailOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly IEntityManager _entManager;
    private readonly SpriteSystem _sprite;

    public TrailOverlay(IEntityManager entManager)
    {
        ZIndex = (int) DrawDepth.Effects;

        _entManager = entManager;
        _sprite = _entManager.System<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var scaleMatrix = Matrix3Helpers.CreateScale(Vector2.One);
        var rotationMatrix = Matrix3Helpers.CreateRotation(-rotation);
        var bounds = args.WorldAABB.Enlarged(5f);

        var query = _entManager.EntityQueryEnumerator<TrailComponent>();
        while (query.MoveNext(out _, out var trail))
        {
            var texture = _sprite.Frame0(trail.Sprite);
            foreach (var data in trail.TrailData)
            {
                var worldPosition = data.Position;
                if (!bounds.Contains(worldPosition))
                    continue;

                var worldMatrix = Matrix3Helpers.CreateTranslation(worldPosition);
                var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
                var matty = Matrix3x2.Multiply(rotationMatrix, scaledWorld);
                handle.SetTransform(matty);

                handle.DrawTexture(texture, -(Vector2) texture.Size / 2f / EyeManager.PixelsPerMeter, data.Angle, data.Color);
            }
        }

        handle.SetTransform(Matrix3x2.Identity);
    }
}
