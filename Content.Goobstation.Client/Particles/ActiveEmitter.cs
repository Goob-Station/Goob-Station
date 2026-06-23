// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Particles;
using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Goobstation.Client.Particles;

/// <summary>
/// A running particle emitter and its live particle pool.
/// Created in <see cref="ParticleSystem"/>.
/// </summary>
public sealed class ActiveEmitter
{
    public ParticleEffectPrototype Proto = default!;

    /// <summary>
    /// Current world-space origin of the emitter.
    /// </summary>
    public MapCoordinates MapCoords;

    /// <summary>
    /// Entity this emitter follows (if any).
    /// </summary>
    public EntityUid? AttachedEntity;

    /// <summary>
    /// Time elapsed since this emitter was created.
    /// </summary>
    public TimeSpan Age;

    /// <summary>
    /// Emission accumulator for sub-tick emission rates.
    /// </summary>
    public float EmitAccum;

    /// <summary>
    /// True once the emitter stops producing new particles. Existing particles live out their lifetimes.
    /// </summary>
    public bool Exhausted;

    /// <summary>
    /// Unique client-side handle for addressing this emitter by ID.
    /// Prefer holding the <see cref="ActiveEmitter"/> reference directly when possible, please.
    /// </summary>
    public uint Handle;

    /// <summary>
    /// Color tint multiplied on top of every particle's computed color.
    /// </summary>
    public Color? ColorOverride;

    /// <summary>
    /// Intensity multiplier for emission rate and particle size. 1.0 = normal.
    /// </summary>
    public float Intensity = 1f;

    /// <summary>
    /// Live overrides shadowing individual prototype fields.
    /// Non-null values take priority, null falls back to the prototype.
    /// </summary>
    public ParticleRuntimeOverrides? Overrides;

    #region Velocity tracking

    public Vector2 PreviousPosition;
    public Vector2 EmitterVelocity;
    public bool VelocityInitialized;

    #endregion

    #region Aim-at targeting

    /// <summary>
    /// When set, each tick the emit angle is recomputed to point toward this entity.
    /// Falls back to <see cref="TargetPosition"/> if the entity is deleted.
    /// </summary>
    public EntityUid? TargetEntity;

    /// <summary>
    /// When set, the emit angle points toward this world position.
    /// Used as a fallback when <see cref="TargetEntity"/> is unset or gone.
    /// </summary>
    public Vector2? TargetPosition;

    /// <summary>
    /// Resolved emit angle in radians, recomputed each tick from the target if one is set.
    /// </summary>
    public float EffectiveEmitAngle;

    #endregion

    #region Timed bursts

    /// <summary>
    /// Tracks which <see cref="ParticleEffectPrototype.Bursts"/> entries have already fired.
    /// </summary>
    public readonly List<bool> FiredBursts = new();

    #endregion

    #region Animation

    /// <summary>
    /// Resolved RSI frames. Populated on creation.
    /// Single-frame sprites have one entry and empty Delays.
    /// </summary>
    public Texture[] Frames = Array.Empty<Texture>();

    /// <summary>
    /// Frame delays when an RSI defines animation.
    /// </summary>
    public float[] Delays = Array.Empty<float>();

    public int AnimFrame;
    public float AnimTimer;

    #endregion

    #region Particles

    // ParticleData objects are never removed from Particles once added.
    // When a particle dies it's marked Alive = false and pushed onto FreePool.
    // The next emission pops from FreePool and resets the object rather than allocating a new one.
    // This avoids GC pressure from short lived allocations during heavy emission.
    // The simulation loop still iterates the full Particles list each frame, so a very large
    // list with mostly dead slots can waste time, <b>emitters should keep MaxCount reasonable.</b>

    /// <summary>
    /// All particle slots, including dead ones held for pooling.
    /// </summary>
    public readonly List<ParticleData> Particles = new();

    /// <summary>
    /// Dead particles available for reuse.
    /// </summary>
    public readonly Queue<ParticleData> FreePool = new();

    public bool HasLiveParticles()
    {
        foreach (var p in Particles)
        {
            if (p.Alive)
                return true;
        }
        return false;
    }

    #endregion
}
