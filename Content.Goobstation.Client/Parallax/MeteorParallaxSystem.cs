using System.Numerics;
using Content.Client.Parallax;
using Content.Client.Parallax.Data;
using Content.Goobstation.Shared.Parallax;
using Content.Shared.CCVar;
using Content.Shared.Parallax.Biomes;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Client.Parallax;

public sealed class MeteorParallaxSystem : EntitySystem
{
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ParallaxSystem _parallax = default!;

    public readonly Dictionary<string, MeteorField> Fields = new();

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new MeteorParallaxOverlay());
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnReload);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<MeteorParallaxOverlay>();
    }

    private void OnReload(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<ParallaxPrototype>())
            Fields.Clear();
    }

    public MeteorField? GetField(MapId mapId)
    {
        var name = _parallax.GetParallax(mapId);
        return Fields.GetValueOrDefault(name);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (!_cfg.GetCVar(CCVars.ParallaxEnabled))
            return;

        if (_eye.CurrentEye is not { } eye)
            return;

        var eyePos = eye.Position.Position;
        var mapId = eye.Position.MapId;
        if (mapId == MapId.Nullspace)
            return;

        if (HasComp<BiomeComponent>(_map.GetMapOrInvalid(mapId)))
            return;

        var name = _parallax.GetParallax(mapId);
        if (!_proto.TryIndex<ParallaxPrototype>(name, out var proto) || proto.Meteors is not { } cfg)
        {
            Fields.Remove(name);
            return;
        }

        if (!Fields.TryGetValue(name, out var field) || field.Config != cfg)
        {
            field = BuildField(cfg);
            Fields[name] = field;
        }

        if (cfg.Style == MeteorParallaxStyle.Sprite && field.Textures.Length == 0)
            return;

        var cullRadius = cfg.SpawnRadius + 6f;
        var cullRadiusSq = cullRadius * cullRadius;

        for (var i = field.Meteors.Count - 1; i >= 0; i--)
        {
            var m = field.Meteors[i];
            m.Offset += m.Velocity * frameTime;

            var screenOffset = (eyePos - m.Home) * (m.Slowness - 1f) + m.Offset;
            if (screenOffset.LengthSquared() > cullRadiusSq)
            {
                field.Meteors.RemoveAt(i);
                continue;
            }

            field.Meteors[i] = m;
        }

        if (cfg.Hero is { } hero)
        {
            field.NextHero -= frameTime;
            if (field.NextHero <= 0f)
            {
                field.Meteors.Add(SpawnHero(cfg, hero, eyePos));
                field.NextHero = _random.NextFloat(hero.IntervalMin, hero.IntervalMax);
            }
        }

        var regular = 0;
        foreach (var m in field.Meteors)
        {
            if (!m.IsHero)
                regular++;
        }

        while (regular < cfg.Count)
        {
            field.Meteors.Add(SpawnMeteor(field, cfg, eyePos, scatter: !field.Initialized));
            regular++;
        }

        field.Initialized = true;
    }

    private MeteorField BuildField(MeteorParallaxConfig cfg)
    {
        var textures = Array.Empty<Texture>();
        if (cfg.Style == MeteorParallaxStyle.Sprite)
        {
            textures = new Texture[cfg.Sprites.Count];
            for (var i = 0; i < cfg.Sprites.Count; i++)
                textures[i] = _sprite.Frame0(cfg.Sprites[i]);
        }

        return new MeteorField
        {
            Config = cfg,
            Textures = textures,
            NextHero = cfg.Hero is { } hero ? _random.NextFloat(hero.IntervalMin, hero.IntervalMax) : 0f,
        };
    }

    private Meteor SpawnMeteor(MeteorField field, MeteorParallaxConfig cfg, Vector2 eyePos, bool scatter)
    {
        var theta = cfg.Direction.Theta + Angle.FromDegrees(_random.NextFloat(-cfg.Spread, cfg.Spread)).Theta;
        var dir = new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
        var velocity = dir * _random.NextFloat(cfg.MinSpeed, cfg.MaxSpeed);

        Vector2 offset;
        if (scatter)
        {
            var a = _random.NextFloat(0f, MathF.Tau);
            var r = cfg.SpawnRadius * MathF.Sqrt(_random.NextFloat());
            offset = new Vector2(MathF.Cos(a), MathF.Sin(a)) * r;
        }
        else
        {
            var incoming = -dir;
            var perp = new Vector2(-incoming.Y, incoming.X);
            offset = incoming * cfg.SpawnRadius + perp * _random.NextFloat(-cfg.SpawnRadius, cfg.SpawnRadius);
        }

        return new Meteor
        {
            Home = eyePos,
            Offset = offset,
            Velocity = velocity,
            Slowness = _random.NextFloat(cfg.MinSlowness, cfg.MaxSlowness),
            Scale = _random.NextFloat(cfg.MinScale, cfg.MaxScale),
            Rotation = cfg.AlignToVelocity ? new Angle(velocity) : Angle.Zero,
            TextureIndex = field.Textures.Length > 0 ? _random.Next(field.Textures.Length) : 0,
            Color = cfg.Colors.Count > 0 ? _random.Pick(cfg.Colors) : cfg.Color,
            Brightness = _random.NextFloat(cfg.MinBrightness, cfg.MaxBrightness),
            Phase = _random.NextFloat(0f, MathF.Tau),
            HeadSize = cfg.HeadSize,
            TrailLength = cfg.TrailLength,
            TrailWidth = cfg.TrailWidth > 0f ? cfg.TrailWidth : cfg.HeadSize,
            GlowSize = cfg.Glow ? cfg.GlowSize : 0f,
            IsHero = false,
        };
    }

    private Meteor SpawnHero(MeteorParallaxConfig cfg, MeteorHeroConfig hero, Vector2 eyePos)
    {
        var theta = cfg.Direction.Theta + Angle.FromDegrees(_random.NextFloat(-cfg.Spread, cfg.Spread)).Theta;
        var dir = new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
        var velocity = dir * hero.Speed;

        var incoming = -dir;
        var perp = new Vector2(-incoming.Y, incoming.X);
        var offset = incoming * cfg.SpawnRadius + perp * _random.NextFloat(-cfg.SpawnRadius, cfg.SpawnRadius);

        return new Meteor
        {
            Home = eyePos,
            Offset = offset,
            Velocity = velocity,
            Slowness = _random.NextFloat(cfg.MinSlowness, cfg.MaxSlowness),
            Scale = hero.Scale,
            Rotation = Angle.Zero,
            TextureIndex = 0,
            Color = hero.Color,
            Brightness = 1f,
            Phase = 0f,
            HeadSize = cfg.HeadSize,
            TrailLength = hero.TrailLength,
            TrailWidth = hero.TrailWidth,
            GlowSize = hero.GlowSize,
            IsHero = true,
        };
    }
}

public sealed class MeteorField
{
    public MeteorParallaxConfig Config = default!;
    public Texture[] Textures = Array.Empty<Texture>();
    public readonly List<Meteor> Meteors = new();
    public bool Initialized;
    public float NextHero;
}

public struct Meteor
{
    public Vector2 Home;
    public Vector2 Offset;
    public Vector2 Velocity;
    public float Slowness;
    public float Scale;
    public Angle Rotation;
    public int TextureIndex;
    public Color Color;
    public float Brightness;
    public float Phase;
    public float HeadSize;
    public float TrailLength;
    public float TrailWidth;
    public float GlowSize;
    public bool IsHero;
}
