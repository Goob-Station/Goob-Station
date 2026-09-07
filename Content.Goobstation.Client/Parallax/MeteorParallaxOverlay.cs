using System.Numerics;
using Content.Client.Parallax;
using Content.Goobstation.Shared.Parallax;
using Content.Shared.CCVar;
using Content.Shared.Parallax.Biomes;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Parallax;

public sealed class MeteorParallaxOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";
    private static readonly ProtoId<ShaderPrototype> GlowShader = "MeteorGlow";

    private readonly MeteorParallaxSystem _meteors;
    private readonly SharedMapSystem _map;
    private readonly ShaderInstance _unshaded;
    private readonly ShaderInstance _glow;

    private readonly DrawVertexUV2DColor[] _trailVerts = new DrawVertexUV2DColor[3];

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowWorld;

    public MeteorParallaxOverlay()
    {
        ZIndex = ParallaxSystem.ParallaxZIndex + 1;
        IoCManager.InjectDependencies(this);
        _meteors = _entManager.System<MeteorParallaxSystem>();
        _map = _entManager.System<SharedMapSystem>();
        _unshaded = _proto.Index(UnshadedShader).Instance();
        _glow = _proto.Index(GlowShader).Instance();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.MapId == MapId.Nullspace)
            return false;

        if (_entManager.HasComponent<BiomeComponent>(_map.GetMapOrInvalid(args.MapId)))
            return false;

        return _meteors.GetField(args.MapId) != null;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_cfg.GetCVar(CCVars.ParallaxEnabled))
            return;

        if (_meteors.GetField(args.MapId) is not { } field)
            return;

        var eyePos = args.Viewport.Eye?.Position.Position ?? Vector2.Zero;
        var handle = args.WorldHandle;
        var cfg = field.Config;
        var time = (float) _timing.RealTime.TotalSeconds;

        handle.UseShader(_unshaded);
        foreach (var m in field.Meteors)
        {
            var center = (eyePos - m.Home) * m.Slowness + m.Home + m.Offset;
            if (cfg.Style == MeteorParallaxStyle.Comet || m.IsHero)
                DrawComet(handle, center, m, cfg, time);
            else
                DrawSprite(handle, center, m, field);
        }

        handle.UseShader(_glow);
        foreach (var m in field.Meteors)
        {
            if (m.GlowSize <= 0f)
                continue;

            var center = (eyePos - m.Home) * m.Slowness + m.Home + m.Offset;
            DrawGlow(handle, center, m, cfg, time);
        }

        handle.UseShader(null);
    }

    private static void DrawSprite(DrawingHandleWorld handle, Vector2 center, Meteor m, MeteorField field)
    {
        var tex = field.Textures[m.TextureIndex];
        var size = tex.Size / (float) EyeManager.PixelsPerMeter * m.Scale;
        var box = Box2.FromDimensions(center - size / 2, size);

        if (m.Rotation == Angle.Zero)
            handle.DrawTextureRect(tex, box, m.Color);
        else
            handle.DrawTextureRect(tex, new Box2Rotated(box, m.Rotation, center), m.Color);
    }

    private void DrawComet(DrawingHandleWorld handle, Vector2 center, Meteor m, MeteorParallaxConfig cfg, float time)
    {
        var speedSq = m.Velocity.LengthSquared();
        var dir = speedSq > 0.0001f ? Vector2.Normalize(m.Velocity) : new Vector2(1f, 0f);
        var perp = new Vector2(-dir.Y, dir.X);

        var head = m.HeadSize * m.Scale;
        var halfWidth = m.TrailWidth * m.Scale * 0.5f;
        var length = m.TrailLength * m.Scale;

        var drawColor = MeteorColor(m, cfg, time);
        var headColor = Color.FromSrgb(drawColor);
        var tailColor = headColor.WithAlpha(0f);

        _trailVerts[0] = new DrawVertexUV2DColor(center + perp * halfWidth, headColor);
        _trailVerts[1] = new DrawVertexUV2DColor(center - perp * halfWidth, headColor);
        _trailVerts[2] = new DrawVertexUV2DColor(center - dir * length, tailColor);
        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, Texture.White, _trailVerts);

        var half = new Vector2(head * 0.5f);
        handle.DrawTextureRect(Texture.White, Box2.FromDimensions(center - half, new Vector2(head)), drawColor);
    }

    private static void DrawGlow(DrawingHandleWorld handle, Vector2 center, Meteor m, MeteorParallaxConfig cfg, float time)
    {
        var size = m.HeadSize * m.Scale * m.GlowSize;
        var half = new Vector2(size * 0.5f);
        handle.DrawTextureRect(Texture.White, Box2.FromDimensions(center - half, new Vector2(size)), MeteorColor(m, cfg, time));
    }

    private static Color MeteorColor(Meteor m, MeteorParallaxConfig cfg, float time)
    {
        var alpha = m.Color.A * m.Brightness;
        if (cfg.Twinkle && !m.IsHero)
            alpha *= 1f - cfg.TwinkleAmount * (0.5f + 0.5f * MathF.Sin(time * cfg.TwinkleSpeed + m.Phase));

        return m.Color.WithAlpha(Math.Clamp(alpha, 0f, 1f));
    }
}
