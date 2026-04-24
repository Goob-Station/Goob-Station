// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Goobstation.Shared.Particles;

/// <summary>
/// Per-emitter runtime overrides for <see cref="ParticleEffectPrototype"/> fields.
/// Every field is nullable — null means "use the prototype value". Only set what you need to change.
/// </summary>
public sealed class ParticleRuntimeOverrides
{
    #region Visuals

    /// <summary>
    /// Self-explanatory lerps to EndColor over the particle's lifetime linearly.
    /// </summary>
    public Color? StartColor;

    public Color? EndColor;

    /// <summary>
    /// Global tint multiplied on top of every particle's color.
    /// </summary>
    public Color? ColorOverride;

    /// <summary>
    /// The shader to use for this emitter's particles. Falls back to the prototype's shader if null.
    /// </summary>
    public string? Shader;

    /// <summary>
    /// The render layer to draw these particles on. Falls back to the prototype's layer if null.
    /// </summary>
    public int? RenderLayer;
    #endregion
    #region Size

    /// <summary>
    /// The base size of each particle.
    /// </summary>
    public float? ParticleSize;

    /// <summary>
    /// Random variance added to each particle's size at spawn, in the range [-SizeVariance, SizeVariance].
    /// </summary>
    public float? SizeVariance;

    /// <summary>
    /// Multiplies the particle's length along its velocity vector, creating a stretched streak effect. 1.0 = normal.
    /// </summary>
    public float? StretchFactor;

    #endregion
    #region Lifetime

    /// <summary>
    /// How long each particle lives before disappearing.
    /// </summary>
    public TimeSpan? Lifetime;

    /// <summary>
    ///  Random variance added to each particle's lifetime at spawn, in the range [-LifetimeVariance, LifetimeVariance].
    /// </summary>
    public TimeSpan? LifetimeVariance;

    #endregion
    #region Movement

    /// <summary>
    /// Initial speed of each particle at spawn.
    /// </summary>
    public float? Speed;

    /// <summary>
    /// Random variance added to each particle's initial speed at spawn, in the range [-SpeedVariance, SpeedVariance].
    /// </summary>
    public float? SpeedVariance;

    /// <summary>
    /// A constant acceleration applied to each particle every tick, X = right, Y = up.
    /// </summary>
    public Vector2? ConstantForce;

    /// <summary>
    /// Additional downward (or upward with negative values) acceleration applied to each particle every tick. Added on top of ConstantForce.Y.
    /// </summary>
    public float? Gravity;

    /// <summary>
    /// Multiplier applied to each particle's velocity every tick, simulating air resistance. 1.0 = no drag, 0.5 = velocity halved every tick.
    /// </summary>
    public float? Drag;

    /// <summary>
    /// Maximum speed for each particle. If non-null, velocity is clamped to this magnitude every tick after applying forces and drag.
    /// </summary>
    public float? TerminalSpeed;

    /// <summary>
    /// Strength of the procedural noise turbulence applied to each particle every tick, in screen-space units. Noise is a curl field based on Perlin noise.
    /// </summary>
    public float? NoiseStrength;

    /// <summary>
    /// Frequency of the procedural noise turbulence. Higher values create smaller, more chaotic swirls, while lower values create larger, smoother waves.
    /// </summary>
    public float? NoiseFrequency;

    /// <summary>
    /// Multiplier for how much of the emitter's current velocity is inherited by each particle at spawn.
    /// 0.0 = no inheritance, 1.0 = particles spawn with the same velocity as the emitter.
    /// </summary>
    public float? InheritVelocity;

    #endregion
    #region Rotation

    /// <summary>
    /// Initial rotation of each particle at spawn, in radians. 0 = facing right, positive = clockwise.
    /// </summary>
    public Angle? StartRotation;

    /// <summary>
    /// Random variance added to each particle's initial rotation at spawn, in radians, in the range [-StartRotationVariance, StartRotationVariance].
    /// </summary>
    public Angle? StartRotationVariance;

    /// <summary>
    /// Spin rate of each particle in radians per second. Positive values spin clockwise, negative values spin counterclockwise.
    /// </summary>
    public Angle? RotationSpeed;

    /// <summary>
    /// Random variance added to each particle's rotation speed at spawn, in radians per second, in the range [-RotationSpeedVariance, RotationSpeedVariance].
    /// </summary>
    public Angle? RotationSpeedVariance;

    #endregion
    #region Emission

    /// <summary>
    ///  Number of particles emitted per second while the emitter is active.
    /// </summary>
    public float? EmissionRate;

    /// <summary>
    /// Max live particles at once. Be careful raising this at runtime, the pool only grows, never shrinks.
    /// Increasing MaxCount after spawn causes new allocations beyond the original pool size.
    /// Decreasing it is safe but the extra slots stay allocated until the emitter is destroyed.
    /// </summary>
    public int? MaxCount;

    /// <summary>
    /// How long the emitter produces new particles before stopping. Existing particles live out their lifetimes. Null means infinite duration.
    /// </summary>
    public TimeSpan? Duration;

    /// <summary>
    ///  Random variance added to each particle's initial movement angle at spawn, in radians, in the range [-SpreadAngle/2, SpreadAngle/2]. 0 means all particles move in the same direction.
    /// </summary>
    public Angle? SpreadAngle;

    /// <summary>
    /// Base movement angle of each particle at spawn, in radians. 0 = facing right, positive = clockwise. SpreadAngle is added on top of this base angle as a random variance.
    /// </summary>
    public Angle? EmitAngle;

    #endregion
}
