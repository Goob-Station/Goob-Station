// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Particles;

/// <summary>
/// Keyframe for a float-over-lifetime curve. Time is normalised 0–1.
/// </summary>
[DataRecord]
public partial record struct ParticleCurveKey
{
    [DataField(required: true)]
    public float Time { get; private set; }

    [DataField(required: true)]
    public float Value { get; private set; }
}

/// <summary>
/// Keyframe for a color-over-lifetime gradient. Time is normalised 0–1.
/// </summary>
[DataRecord]
public partial record struct ColorCurveKey
{
    /// <summary>
    /// Time along the particle's lifetime (0–1). 0 = birth, 1 = death.
    /// </summary>
    [DataField(required: true)]
    public float Time { get; private set; }

    /// <summary>
    /// Color at this point in the lifetime. Alpha channel is respected and multiplied with the alpha curve if present.
    /// </summary>
    [DataField(required: true)]
    public Color Color { get; private set; }
}

/// <summary>
/// Keyframe for a Vector2-over-lifetime curve. Time is normalised 0–1.
/// </summary>
[DataRecord]
public partial record struct Vector2CurveKey
{
    /// <summary>
    /// Time along the particle's lifetime (0–1). 0 = birth, 1 = death.
    /// </summary>
    [DataField(required: true)]
    public float Time { get; private set; }

    /// <summary>
    /// Vector2 value at this point in the lifetime. Interpretation depends on the context of the curve (force, velocity, etc).
    /// </summary>
    [DataField(required: true)]
    public Vector2 Value { get; private set; }
}

/// <summary>
/// Fires <see cref="Count"/> particles at <see cref="Time"/> after the emitter starts.
/// </summary>
[DataRecord]
public partial record struct ParticleBurstData()
{
    [DataField]
    public TimeSpan Time { get; private set; }

    [DataField]
    public int Count { get; private set; } = 10;
}

public enum EmissionShapeType : byte
{
    Point,      // All particles spawn at the emitter origin.
    CircleEdge, // Particles spawn randomly along the circumference of a circle with radius.
    CircleFill, // Particles spawn randomly within a circle with radius.
    Box,        // Particles spawn randomly within a rectangle.
}

[DataRecord]
public partial record struct EmissionShapeData()
{
    /// <summary>
    /// Default to emitter's position.
    /// </summary>
    [DataField]
    public EmissionShapeType Type { get; private set; } = EmissionShapeType.Point;

    /// <summary>
    /// For circle shapes, radius of the circle. For box shape, half-extents of the rectangle.
    /// </summary>
    [DataField]
    public float Radius { get; private set; } = 0.5f;

    /// <summary>
    /// For box shape, half-extents of the rectangle. X = width/2, Y = height/2.
    /// </summary>
    [DataField]
    public Vector2 BoxExtents { get; private set; } = new(0.5f, 0.5f);
}

/// <summary>
/// Defines a reusable particle effect prototype.
/// </summary>
[Prototype]
public sealed partial class ParticleEffectPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ParticleEffectPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    #region Visuals

    /// <summary>
    /// Texture drawn for each particle. Supports RSI states and plain texture paths.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Sprite { get; private set; } = default!;

    /// <summary>
    /// Particle color at the start of its life. Ignored when <see cref="ColorOverLifetime"/> is set.
    /// </summary>
    [DataField]
    public Color StartColor { get; private set; } = Color.White;

    /// <summary>
    /// Particle color at the end of its life. Ignored when <see cref="ColorOverLifetime"/> is set.
    /// </summary>
    [DataField]
    public Color EndColor { get; private set; } = Color.Transparent;

    /// <summary>
    /// Multi-stop color gradient over lifetime. Overrides the <see cref="StartColor"/>/<see cref="EndColor"/> lerp when non-empty.
    /// Each key has a normalised time (0–1) and a color.
    /// </summary>
    [DataField]
    public List<ColorCurveKey> ColorOverLifetime { get; private set; } = new();

    /// <summary>
    /// Alpha curve over lifetime (0–1). Multiplied on top of the color's alpha channel.
    /// Leave empty to rely on alpha baked into the colors directly.
    /// </summary>
    [DataField]
    public List<ParticleCurveKey> AlphaOverLifetime { get; private set; } = new();

    /// <summary>
    /// Shader to apply when drawing particles.
    /// </summary>
    [DataField]
    public string? Shader { get; private set; }

    /// <summary>
    /// Draw order. Higher values render on top.
    /// </summary>
    [DataField]
    public int RenderLayer { get; private set; }

    /// <summary>
    /// When true, this particle effect always renders at full quality regardless of user settings.
    /// Use this ONLY for gameplay-critical particles.
    /// Purely cosmetic effects (sparks, smoke, fire) should leave this false.
    /// ᓚᘏᗢ <( <b>If I see you set this to true on a purely cosmetic effect, I will find you and I will hurt you.</b>
    /// </summary>
    [DataField]
    public bool IgnoreQualitySettings { get; private set; }

    #endregion
    #region Size

    /// <summary>
    /// Base particle size in world units.
    /// </summary>
    [DataField]
    public float ParticleSize { get; private set; } = 0.2f;

    /// <summary>
    /// Per-particle size randomization.
    /// </summary>
    [DataField]
    public float SizeVariance { get; private set; }

    /// <summary>
    /// Size multiplier curve over lifetime. Leave empty for constant size.
    /// </summary>
    [DataField]
    public List<ParticleCurveKey> SizeOverLifetime { get; private set; } = new();

    /// <summary>
    /// Stretches particles along their velocity direction.
    /// 0 = no stretch. Higher values give a motion-blur style trail.
    /// </summary>
    [DataField]
    public float StretchFactor { get; private set; }

    #endregion
    #region Lifetime

    /// <summary>
    /// How long each particle lives.
    /// </summary>
    [DataField]
    public TimeSpan Lifetime { get; private set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Per-particle lifetime variance.
    /// </summary>
    [DataField]
    public TimeSpan LifetimeVariance { get; private set; } = TimeSpan.FromSeconds(0.2);

    #endregion
    #region Movement

    /// <summary>
    /// Base spawn speed in world units per second.
    /// </summary>
    [DataField]
    public float Speed { get; private set; } = 1f;

    /// <summary>
    /// Per-particle speed variance.
    /// </summary>
    [DataField]
    public float SpeedVariance { get; private set; } = 0.3f;

    /// <summary>
    /// Speed multiplier curve over lifetime. Leave empty for constant speed.
    /// </summary>
    [DataField]
    public List<ParticleCurveKey> SpeedOverLifetime { get; private set; } = new();

    /// <summary>
    /// Constant velocity added to every particle each frame (world units/sec).
    /// Screen-space: X = right, Y = up.
    /// </summary>
    [DataField]
    public Vector2 ConstantForce { get; private set; }

    /// <summary>
    /// Time-varying force added to velocity over the particle's lifetime.
    /// Sampled by normalized age (0–1), scaled by dt each frame.
    /// Screen-space: X = right, Y = up.
    /// </summary>
    [DataField]
    public List<Vector2CurveKey> ForceOverLifetime { get; private set; } = new();

    /// <summary>
    /// Positional nudge applied over lifetime, adds directly to position, not velocity.
    /// Useful for swirl or curl motion without altering the underlying velocity.
    /// Screen-space: X = right, Y = up.
    /// </summary>
    [DataField]
    public List<Vector2CurveKey> VelocityOverLifetime { get; private set; } = new();

    /// <summary>
    /// Downward drift in world units/sec, applied to position (not velocity).
    /// Negative values make particles float upward.
    /// </summary>
    [DataField]
    public float Gravity { get; private set; }

    /// <summary>
    /// Exponential drag coefficient applied to velocity. 0 = no drag.
    /// </summary>
    [DataField]
    public float Drag { get; private set; }

    /// <summary>
    /// Speed cap in world units/sec. 0 = no cap.
    /// </summary>
    [DataField]
    public float TerminalSpeed { get; private set; }

    /// <summary>
    /// Turbulence strength in world units/sec. 0 = off.
    /// Pair with <see cref="NoiseFrequency"/> to control jitter speed.
    /// </summary>
    [DataField]
    public float NoiseStrength { get; private set; }

    /// <summary>
    /// Animation speed of the noise field. Higher = faster turbulence.
    /// </summary>
    [DataField]
    public float NoiseFrequency { get; private set; } = 1f;

    /// <summary>
    /// Fraction of the emitter's velocity inherited by new particles (0–1).
    /// 1 = particles launch with the full emitter velocity, leaving trails.
    /// </summary>
    [DataField]
    public float InheritVelocity { get; private set; }

    #endregion
    #region Rotation

    /// <summary>
    /// Starting rotation in degrees.
    /// </summary>
    [DataField]
    public Angle StartRotation { get; private set; }

    /// <summary>
    /// Per-particle starting rotation variance in degrees. 180 = fully random.
    /// </summary>
    [DataField]
    public Angle StartRotationVariance { get; private set; }

    /// <summary>
    /// Spin speed in degrees per second.
    /// </summary>
    [DataField]
    public Angle RotationSpeed { get; private set; }

    /// <summary>
    /// Per-particle spin speed variance in degrees per second.
    /// </summary>
    [DataField]
    public Angle RotationSpeedVariance { get; private set; }

    #endregion
    #region Emission

    /// <summary>
    /// Particles emitted per second. Ignored when <see cref="Burst"/> is true or <see cref="Bursts"/> is non-empty.
    /// </summary>
    [DataField]
    public float EmissionRate { get; private set; } = 20f;

    /// <summary>
    /// Emission rate multiplier curve over the emitter's duration.
    /// When <see cref="Duration"/> > 0, t = age / duration. When duration is zero (infinite), t clamps to 1 after 1 second.
    /// </summary>
    [DataField]
    public List<ParticleCurveKey> EmissionOverTime { get; private set; } = new();

    /// <summary>
    /// Max live particles this emitter can have at once.
    /// Set this to roughly the highest number of particles you expect to see on screen at one time, not the total
    /// spawned over the effect's lifetime. Slots are allocated up front and never freed until the emitter dies,
    /// so a value of 500 on a slow effect that only ever has 10 visible particles wastes 490 slots of memory.
    /// <b>Keep It Low.</b>
    /// </summary>
    [DataField]
    public int MaxCount { get; private set; } = 50;

    /// <summary>
    /// When true, emits all <see cref="MaxCount"/> particles at once then stops immediately.
    /// </summary>
    [DataField]
    public bool Burst { get; private set; }

    /// <summary>
    /// Timed burst entries. Can be combined with continuous emission.
    /// </summary>
    [DataField]
    public List<ParticleBurstData> Bursts { get; private set; } = new();

    /// <summary>
    /// How long the emitter runs. 0 = forever.
    /// </summary>
    [DataField]
    public TimeSpan Duration { get; private set; }

    #endregion
    #region Space

    /// <summary>
    /// When true (default), particles simulate in world space and trail behind moving emitters.
    /// When false, particles move relative to the emitter origin.
    /// </summary>
    [DataField]
    public bool WorldSpace { get; private set; } = true;

    #endregion
    #region Shape

    [DataField]
    public EmissionShapeData Shape { get; private set; } = new();

    #endregion
    #region Angle

    /// <summary>
    /// Emission cone spread in degrees. 360 = omnidirectional.
    /// </summary>
    [DataField]
    public Angle SpreadAngle { get; private set; } = Angle.FromDegrees(360);

    /// <summary>
    /// Emission direction bias in degrees. 0 = screen-up.
    /// </summary>
    [DataField]
    public Angle EmitAngle { get; private set; }

    #endregion
    #region Sub-emitters

    /// <summary>
    /// Spawns this effect at each particle's position when it dies.
    /// </summary>
    [DataField]
    public ProtoId<ParticleEffectPrototype>? SubEmitterOnDeath { get; private set; }

    /// <summary>
    /// Spawns this effect at each particle's position when it spawns.
    /// </summary>
    [DataField]
    public ProtoId<ParticleEffectPrototype>? SubEmitterOnSpawn { get; private set; }
    #endregion
}
