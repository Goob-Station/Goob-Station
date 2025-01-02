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
        var bounds = args.WorldAABB.Enlarged(5f);

        var query = _entManager.EntityQueryEnumerator<TrailComponent>();
        while (query.MoveNext(out _, out var trail))
        {
            var texture = _sprite.Frame0(trail.Sprite);
            var pos = -(Vector2) texture.Size / 2f / EyeManager.PixelsPerMeter;
            foreach (var data in trail.TrailData)
            {
                var worldPosition = data.Position;
                if (!bounds.Contains(worldPosition))
                    continue;

                handle.SetTransform(Matrix3Helpers.CreateTranslation(worldPosition));
                handle.DrawTexture(texture, pos, data.Angle, data.Color);
            }
        }

        handle.SetTransform(Matrix3x2.Identity);
    }
}
