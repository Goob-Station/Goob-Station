using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Parallax;

[DataDefinition]
public sealed partial class MeteorParallaxConfig
{
    [DataField]
    public MeteorParallaxStyle Style = MeteorParallaxStyle.Sprite;

    [DataField]
    public List<SpriteSpecifier> Sprites = new()
    {
        new SpriteSpecifier.Rsi(new ResPath("Objects/Misc/meteor.rsi"), "small"),
        new SpriteSpecifier.Rsi(new ResPath("Objects/Misc/meteor.rsi"), "medium"),
        new SpriteSpecifier.Rsi(new ResPath("Objects/Misc/meteor.rsi"), "space_dust"),
    };

    [DataField]
    public int Count = 12;

    [DataField]
    public Angle Direction = Angle.FromDegrees(225);

    [DataField]
    public float Spread = 12f;

    [DataField]
    public float MinSpeed = 3f;

    [DataField]
    public float MaxSpeed = 8f;

    [DataField]
    public float MinScale = 0.5f;

    [DataField]
    public float MaxScale = 1.5f;

    [DataField]
    public float MinSlowness = 0.9f;

    [DataField]
    public float MaxSlowness = 0.98f;

    [DataField]
    public float SpawnRadius = 24f;

    [DataField]
    public bool AlignToVelocity = true;

    [DataField]
    public Color Color = Color.White;

    [DataField]
    public List<Color> Colors = new();

    [DataField]
    public float TrailLength = 2.5f;

    [DataField]
    public float HeadSize = 0.18f;

    [DataField]
    public float TrailWidth;

    [DataField]
    public float MinBrightness = 1f;

    [DataField]
    public float MaxBrightness = 1f;

    [DataField]
    public bool Twinkle;

    [DataField]
    public float TwinkleSpeed = 3f;

    [DataField]
    public float TwinkleAmount = 0.4f;

    [DataField]
    public bool Glow;

    [DataField]
    public float GlowSize = 2.5f;

    [DataField]
    public MeteorHeroConfig? Hero;
}

[DataDefinition]
public sealed partial class MeteorHeroConfig
{
    [DataField]
    public float IntervalMin = 40f;

    [DataField]
    public float IntervalMax = 120f;

    [DataField]
    public float Speed = 14f;

    [DataField]
    public float Scale = 1.5f;

    [DataField]
    public float TrailLength = 9f;

    [DataField]
    public float TrailWidth = 0.28f;

    [DataField]
    public Color Color = Color.White;

    [DataField]
    public float GlowSize = 4f;
}

public enum MeteorParallaxStyle : byte
{
    Sprite,
    Comet,
}
