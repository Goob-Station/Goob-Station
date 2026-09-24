using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Goobstation.Client.Projectiles;

public sealed class SpeedLinesOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> ShaderProto = "SpeedLines";

    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private readonly SharedTransformSystem _xformSystem;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowEntities;

    public SpeedLinesOverlay()
    {
        IoCManager.InjectDependencies(this);
        _xformSystem = _entMan.System<SharedTransformSystem>();
        _shader = _protoMan.Index(ShaderProto).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _entMan.Count<SpeedLinesEffectComponent>() > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;

        var query = _entMan.AllEntityQueryEnumerator<SpeedLinesEffectComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var lines, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var seed = lines.Seed;
            var worldPos = _xformSystem.GetWorldPosition(uid);
            var direction = lines.Direction;
            if (direction == Vector2.Zero)
                direction = Vector2.UnitX;
            direction = direction.Normalized();

            var spriteOffset = 0f;
            if (_entMan.TryGetComponent<SpriteComponent>(uid, out var sprite))
                spriteOffset = MathF.Max(0f, Vector2.Dot(sprite.Offset, direction)) / (lines.Size * 0.5f);

            var screenCenter = args.Viewport.WorldToLocal(worldPos);
            screenCenter.Y = args.Viewport.Size.Y - screenCenter.Y;

            var pixelsPerMeter = EyeManager.PixelsPerMeter * args.Viewport.RenderScale;
            var screenSize = new Vector2(lines.Size, lines.Size) * pixelsPerMeter;

            _shader.SetParameter("center", screenCenter);
            _shader.SetParameter("size", screenSize);
            _shader.SetParameter("seed", seed);
            _shader.SetParameter("direction", direction);
            _shader.SetParameter("sprOffset", spriteOffset);
            _shader.SetParameter("progress", lines.Progress);
            _shader.SetParameter("color", new Vector4(lines.Color.R, lines.Color.G, lines.Color.B, lines.Color.A));

            var worldBox = Box2.CenteredAround(worldPos, new Vector2(lines.Size, lines.Size));

            worldHandle.UseShader(_shader);
            worldHandle.DrawRect(worldBox, Color.White);
            worldHandle.UseShader(null);
        }
    }
}
