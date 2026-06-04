// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Particles;

namespace Content.Goobstation.Client.Particles;

/// <summary>
/// A single live particle. Class so it can be pooled in place.
/// </summary>
public sealed class ParticleData
{
    /// <summary>
    /// Screen-space offset from the emitter origin, X = right, Y = up.
    /// All simulation runs in this space regardless of <see cref="ParticleEffectPrototype.WorldSpace"/>.
    /// </summary>
    public Vector2 LocalOffset;

    /// <summary>
    /// World position at spawn time, used for world-space particles.
    /// Draw position = SpawnOrigin + rotate(LocalOffset, -eyeRot).
    /// Unused for screen-space particles.
    /// </summary>
    public Vector2 SpawnOrigin;

    /// <summary>
    /// Current movement vector in screen-space units/sec
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    /// How long this particle has been alive
    /// </summary>
    public TimeSpan Age;

    /// <summary>
    /// Total lifespan before the particle dies
    /// </summary>
    public TimeSpan Lifetime;

    /// <summary>
    /// Speed magnitude at spawn, used by SpeedOverLifetime
    /// </summary>
    public float SpawnSpeed;

    /// <summary>
    /// Emitter intensity captured at spawn, used to scale rendered size
    /// </summary>
    public float SpawnIntensity;

    /// <summary>
    /// Current rotation in radians
    /// </summary>
    public float Rotation;

    /// <summary>
    /// Spin rate in radians per second
    /// </summary>
    public float RotationSpeed;

    /// <summary>
    ///  False = dead and available for pooling
    /// </summary>
    public bool Alive;

    /// <summary>
    /// Size multiplier baked in at spawn from SizeVariance.
    /// </summary>
    public float SizeMultiplier = 1f;

    /// <summary>
    /// Noise seed so each particle gets different turbulence.
    /// </summary>
    public Vector2 NoiseOffset;

    public float AgeRatio => Lifetime > TimeSpan.Zero ? Math.Clamp((float)(Age.TotalSeconds / Lifetime.TotalSeconds), 0f, 1f) : 1f;

    public void Reset()
    {
        Age = TimeSpan.Zero;
        Alive = false;
    }
}
