// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Particles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Particles;

/// <summary>
/// Manages active particle emitters on the client, including their simulation and rendering via <see cref="ParticleOverlay"/>.
/// </summary>
public sealed partial class ParticleSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IResourceCache _resource = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly List<ActiveEmitter> _emitters = new();
    private readonly List<(ProtoId<ParticleEffectPrototype> Id, MapCoordinates Coords)> _pendingSubEmitters = new();
    private readonly Dictionary<ParticleEffectPrototype, (Texture[] Frames, float[] Delays)> _frameCache = new();
    private readonly HashSet<ParticleEffectPrototype> _frameResolveFailures = new();
    private ParticleOverlay _particleOverlay = default!;

    private int _quality;

    /// <summary>
    /// Manual override from <see cref="GoobCVars.ParticleGlobalBudget"/>.
    /// </summary>
    private int _manualBudget;

    /// <summary>
    /// Effective global budget for the current frame, derived from <see cref="_quality"/> and <see cref="_manualBudget"/>.
    /// </summary>
    private int _globalBudget;

    /// <summary>
    /// Running tally of <see cref="ParticleEffectPrototype.IgnoreQualitySettings"/> emitters in
    /// <see cref="_emitters"/> so the quality-0 spawn cap doesn't have to scan the whole list every frame.
    /// </summary>
    private int _ignoreQualityEmitterCount;

    /// <summary>
    /// Running tally of all live particles across every emitter. Incremented in
    /// <see cref="EmitParticle"/>, decremented anywhere a particle's alive state gets set to false.
    /// </summary>
    private int _liveParticleCount;

    private static readonly Predicate<ActiveEmitter> RemoveNonIgnoreQualityPredicate =
        e => !e.Proto.IgnoreQualitySettings;

    private const float TeleportThresholdSquared = 16f * 16f;

    private const float MinParticleLifetimeSeconds = 0.05f;

    /// <summary>
    /// Eye view-bounds buffer used by the simulation. Off-screen emitters within this padded box
    /// still simulate so particles already in flight aren't frozen at the screen edge.
    /// </summary>
    private const float ViewBoundsPaddingFactor = 1.5f;

    private const int MaxIgnoreQualityEmittersAtZero = 8;

    /// <summary>
    /// Hard cap on sub-emitter spawns chained from a single tick. Stops
    /// SubEmitterOnSpawn -> SubEmitterOnSpawn cycles killing the client down with infinite recursion :(
    /// </summary>
    private const int MaxSubEmittersPerTick = 256;

    /// <summary>
    /// Incrementing handle counter. Old values are abandoned when emitters die, handles are never reused.
    /// uint gives ~4 billion spawns before wrapping, if this causes problems, you scare me.
    /// </summary>
    private uint _nextHandle = 1;

    /// <summary>
    /// Emission/count multipliers per quality level: Off, Low, Medium, High.
    /// So when quality is set to Low, only 25% of the particles spawn, at Medium it's 50%, and at High it's 100%.
    /// </summary>
    private static readonly float[] QualityMultipliers = { 0f, 0.25f, 0.5f, 1f };

    /// <summary>
    /// Default global particle budgets per quality level.
    /// </summary>
    private static readonly int[] QualityBudgets = { 0, 2250, 5500, 10000, 20000 };

    /// <summary>
    /// Absolute ceiling on live particles regardless of quality settings or anything else.
    /// In isolated testing, I was able to spawn ~26,000 simultaneous particles before significant frame drops.
    /// This number is NOT a target and MUST NOT be treated as one and is intentionally set well below that for several reasons:
    /// <list type="bullet">
    ///   <item><b>All particle simulation runs entirely on the CPU</b>. Every particle competes
    ///   with gameplay logic, physics, networking, and rendering on the same thread.</item>
    ///   <item>That 26k figure was measured in isolation. In a real round with entities, atmos, and players,
    ///   performance will degrade significantly sooner.</item>
    ///   <item>Emitters stack multiplicatively. Ten "small" effects at 500 particles each is already
    ///   5,000 particles before considering anything else in the scene.</item>
    /// </list>
    /// Rendering is submitted as one batched GPU draw call per emitter via <see cref="ParticleOverlay"/>,
    /// so the bottleneck is purely CPU-side simulation. Offloading the physics step to the GPU would require
    /// engine changes, and we aint doing allat.
    /// <b>Do not raise this limit just because your machine can handle it.</b>
    /// This limit exists to protect performance across all hardware and real gameplay conditions.
    /// If you believe this needs to be increased, you should first justify why the effect cannot
    /// be achieved more efficiently. You do NOT need that many particles.
    /// </summary>
    private const int HardMaxParticles = 10000;

    /// <summary>
    /// Maximum particles per emitter for <see cref="ParticleEffectPrototype.IgnoreQualitySettings"/> effects
    /// when quality is below High. At High quality they respect the full <see cref="HardMaxParticles"/> ceiling.
    /// </summary>
    private const int IgnoreQualityMaxParticles = 64;

    #region Particle System API
    public override void Initialize()
    {
        base.Initialize();

        _particleOverlay = new ParticleOverlay(this);
        _overlay.AddOverlay(_particleOverlay);

        _cfg.OnValueChanged(GoobCVars.ParticleQuality, OnQualityChanged, invokeImmediately: true);
        _cfg.OnValueChanged(GoobCVars.ParticleGlobalBudget, OnManualBudgetChanged, invokeImmediately: true);

        _proto.PrototypesReloaded += OnPrototypesReloaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(GoobCVars.ParticleQuality, OnQualityChanged);
        _cfg.UnsubValueChanged(GoobCVars.ParticleGlobalBudget, OnManualBudgetChanged);
        _proto.PrototypesReloaded -= OnPrototypesReloaded;
        _overlay.RemoveOverlay(_particleOverlay);
        _emitters.Clear();
        _pendingSubEmitters.Clear();
        _frameCache.Clear();
        _frameResolveFailures.Clear();
        _ignoreQualityEmitterCount = 0;
        _liveParticleCount = 0;
    }

    private void OnQualityChanged(int quality)
    {
        _quality = quality;
        RecomputeBudget();
    }

    private void OnManualBudgetChanged(int budget)
    {
        _manualBudget = budget;
        RecomputeBudget();
    }


    private void RecomputeBudget()
    {
        var qualityBudget = _quality >= 0 && _quality < QualityBudgets.Length
            ? QualityBudgets[_quality]
            : 0;

        _globalBudget = _quality >= QualityBudgets.Length - 1
            ? Math.Min(_manualBudget, HardMaxParticles)
            : qualityBudget;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<ParticleEffectPrototype>())
            return;
        _frameCache.Clear();
        _frameResolveFailures.Clear();
    }

    public IReadOnlyList<ActiveEmitter> GetEmitters() => _emitters;

    /// <summary>
    /// Immediately destroys all active emitters and kills every live particle.
    /// This is the nuclear option, use it when something has gone very wrong.
    /// </summary>
    /// <returns>Number of emitters that were cleared.</returns>
    public int KillAll()
    {
        var count = _emitters.Count;
        _emitters.Clear();
        _pendingSubEmitters.Clear();
        _ignoreQualityEmitterCount = 0;
        _liveParticleCount = 0;
        return count;
    }

    /// <summary>
    /// Spawns a particle effect at a given map coordinate.
    /// </summary>
    public ActiveEmitter? SpawnEffect(
        [ForbidLiteral] ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        EntityUid? attachedEntity = null,
        Color? colorOverride = null)
    {
        if (!_proto.Resolve(effectId, out var proto))
            return null;

        // Skip quality check if this is a gameplay-critical particle
        if (_quality == 0)
        {
            if (!proto.IgnoreQualitySettings)
                return null;

            // Even IgnoreQualitySettings effects are capped at 8 simultaneous emitters when quality is Off.
            if (_ignoreQualityEmitterCount >= MaxIgnoreQualityEmittersAtZero)
                return null;
        }

        var emitter = CreateEmitter(proto, coords, attachedEntity);
        emitter.ColorOverride = colorOverride;

        _emitters.Add(emitter);
        if (proto.IgnoreQualitySettings)
            _ignoreQualityEmitterCount++;

        if (proto.Burst)
        {
            BurstEmit(emitter);
            emitter.Exhausted = true;
        }

        return emitter;
    }

    /// <summary>
    /// Patches runtime overrides on a live emitter by handle.
    /// Only non-null fields are applied, null fields are left unchanged.
    /// </summary>
    public void UpdateRuntime(uint handle, ParticleRuntimeOverrides overrides)
    {
        if (handle == 0)
            return;
        foreach (var emitter in _emitters)
        {
            if (emitter.Handle == handle)
            {
                ApplyOverrides(emitter, overrides);
                break;
            }
        }
    }

    /// <summary>
    /// Patches runtime overrides on a live emitter by direct reference.
    /// Use this when you already have the <see cref="ActiveEmitter"/> from <see cref="SpawnEffect"/>.
    /// </summary>
    public static void UpdateRuntime(ActiveEmitter emitter, ParticleRuntimeOverrides overrides)
        => ApplyOverrides(emitter, overrides);

    private static void ApplyOverrides(ActiveEmitter emitter, ParticleRuntimeOverrides src)
    {
        emitter.Overrides ??= new ParticleRuntimeOverrides();
        var dst = emitter.Overrides;
        dst.StartColor = src.StartColor ?? dst.StartColor;
        dst.EndColor = src.EndColor ?? dst.EndColor;
        dst.ColorOverride = src.ColorOverride ?? dst.ColorOverride;
        dst.Shader = src.Shader ?? dst.Shader;
        dst.RenderLayer = src.RenderLayer ?? dst.RenderLayer;
        dst.ParticleSize = src.ParticleSize ?? dst.ParticleSize;
        dst.SizeVariance = src.SizeVariance ?? dst.SizeVariance;
        dst.StretchFactor = src.StretchFactor ?? dst.StretchFactor;
        dst.Lifetime = src.Lifetime ?? dst.Lifetime;
        dst.LifetimeVariance = src.LifetimeVariance ?? dst.LifetimeVariance;
        dst.Speed = src.Speed ?? dst.Speed;
        dst.SpeedVariance = src.SpeedVariance ?? dst.SpeedVariance;
        dst.ConstantForce = src.ConstantForce ?? dst.ConstantForce;
        dst.Gravity = src.Gravity ?? dst.Gravity;
        dst.Drag = src.Drag ?? dst.Drag;
        dst.TerminalSpeed = src.TerminalSpeed ?? dst.TerminalSpeed;
        dst.NoiseStrength = src.NoiseStrength ?? dst.NoiseStrength;
        dst.NoiseFrequency = src.NoiseFrequency ?? dst.NoiseFrequency;
        dst.InheritVelocity = src.InheritVelocity ?? dst.InheritVelocity;
        dst.StartRotation = src.StartRotation ?? dst.StartRotation;
        dst.StartRotationVariance = src.StartRotationVariance ?? dst.StartRotationVariance;
        dst.RotationSpeed = src.RotationSpeed ?? dst.RotationSpeed;
        dst.RotationSpeedVariance = src.RotationSpeedVariance ?? dst.RotationSpeedVariance;
        dst.EmissionRate = src.EmissionRate ?? dst.EmissionRate;
        dst.MaxCount = src.MaxCount ?? dst.MaxCount;
        dst.Duration = src.Duration ?? dst.Duration;
        dst.SpreadAngle = src.SpreadAngle ?? dst.SpreadAngle;

        if (src.EmitAngle is { } emitAngle)
        {
            dst.EmitAngle = emitAngle;
            if (emitter.TargetEntity == null && emitter.TargetPosition == null)
                emitter.EffectiveEmitAngle = (float) emitAngle.Theta;
        }
    }

    /// <summary>
    /// Spawns a particle effect whose emission direction tracks a target entity each tick.
    /// When the entity is deleted the emitter retains its last angle.
    /// </summary>
    public ActiveEmitter? SpawnEffectAimAt(
        [ForbidLiteral] ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        EntityUid targetEntity,
        EntityUid? attachedEntity = null)
    {
        Vector2? targetPos = null;
        if (!Deleted(targetEntity))
            targetPos = _transform.GetMapCoordinates(targetEntity).Position;
        return SpawnEffectAimedInternal(effectId, coords, targetEntity, targetPos, attachedEntity);
    }

    /// <summary>
    /// Spawns a particle effect whose emission direction points at a fixed world position.
    /// </summary>
    public ActiveEmitter? SpawnEffectAimAt(
        ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        Vector2 targetWorldPosition,
        EntityUid? attachedEntity = null)
    {
        return SpawnEffectAimedInternal(effectId, coords, null, targetWorldPosition, attachedEntity);
    }

    private ActiveEmitter? SpawnEffectAimedInternal(
        ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        EntityUid? targetEntity,
        Vector2? targetPosition,
        EntityUid? attachedEntity)
    {
        if (!_proto.Resolve(effectId, out var proto))
            return null;

        if (_quality == 0)
        {
            if (!proto.IgnoreQualitySettings)
                return null;
            if (_ignoreQualityEmitterCount >= MaxIgnoreQualityEmittersAtZero)
                return null;
        }

        var emitter = CreateEmitter(proto, coords, attachedEntity);
        emitter.TargetEntity = targetEntity;
        emitter.TargetPosition = targetPosition;

        // Resolve aim angle BEFORE the burst so the burst actually points where it should.
        if (targetPosition.HasValue)
        {
            var eyeAngle = (float) _eye.CurrentEye.Rotation;
            var worldDir = targetPosition.Value - emitter.MapCoords.Position;
            if (worldDir.LengthSquared() > 0.0001f)
            {
                var cosE = MathF.Cos(eyeAngle);
                var sinE = MathF.Sin(eyeAngle);
                var sx = worldDir.X * cosE - worldDir.Y * sinE;
                var sy = worldDir.X * sinE + worldDir.Y * cosE;
                emitter.EffectiveEmitAngle = MathF.Atan2(sx, sy);
            }
        }

        _emitters.Add(emitter);
        if (proto.IgnoreQualitySettings)
            _ignoreQualityEmitterCount++;

        if (proto.Burst)
        {
            BurstEmit(emitter);
            emitter.Exhausted = true;
        }

        return emitter;
    }

    public override void FrameUpdate(float frameTime)
    {
        _pendingSubEmitters.Clear();

        // If particles are fully disabled, drop all emitters except those flagged to ignore quality settings.
        if (_quality == 0)
        {
            for (var i = _emitters.Count - 1; i >= 0; i--)
            {
                var em = _emitters[i];
                if (!RemoveNonIgnoreQualityPredicate(em))
                    continue;

                foreach (var p in em.Particles)
                    if (p.Alive)
                        _liveParticleCount--;
                SwapRemoveAt(i);
            }
            if (_emitters.Count == 0)
                return;
        }

        var eye = _eye.CurrentEye;
        var eyeAngle = (float) eye.Rotation;
        var currentMapId = eye.Position.MapId;
        var ageCheck = TimeSpan.FromSeconds(frameTime);

        var viewport = _eye.GetWorldViewport();
        var pad = MathF.Max(viewport.Width, viewport.Height) * (ViewBoundsPaddingFactor - 1f) * 0.5f;
        var viewBounds = viewport.Enlarged(pad);

        for (var i = _emitters.Count - 1; i >= 0; i--)
        {
            var emitter = _emitters[i];

            UpdateEmitterPosition(emitter, frameTime);
            AdvanceEmitterAge(emitter, ageCheck);

            var inView = emitter.MapCoords.MapId == currentMapId
                && viewBounds.Contains(emitter.MapCoords.Position);

            if (inView)
                TickEmitter(emitter, frameTime, eyeAngle);
            else
                AgeOffScreenParticles(emitter, ageCheck);

            if (emitter.Exhausted && !emitter.HasLiveParticles())
            {
                if (emitter.Proto.IgnoreQualitySettings)
                    _ignoreQualityEmitterCount--;
                SwapRemoveAt(i);
            }
        }

        var processed = 0;
        for (var subIdx = 0; subIdx < _pendingSubEmitters.Count; subIdx++)
        {
            if (processed++ >= MaxSubEmittersPerTick)
            {
                Log.Warning($"Particle sub-emitter cap of {MaxSubEmittersPerTick} hit in one frame; dropping remaining spawns. Likely a SubEmitterOnSpawn cycle.");
                break;
            }
            var (id, coords) = _pendingSubEmitters[subIdx];
            SpawnEffect(id, coords);
        }
    }

    private void SwapRemoveAt(int index)
    {
        var last = _emitters.Count - 1;
        if (index != last)
            _emitters[index] = _emitters[last];
        _emitters.RemoveAt(last);
    }

    private void UpdateEmitterPosition(ActiveEmitter emitter, float dt)
    {
        if (emitter.AttachedEntity is not { } attachedEnt)
        {
            if (!emitter.VelocityInitialized)
            {
                emitter.PreviousPosition = emitter.MapCoords.Position;
                emitter.VelocityInitialized = true;
            }
            return;
        }

        if (Deleted(attachedEnt))
        {
            emitter.Exhausted = true;
            emitter.AttachedEntity = null;
            return;
        }

        var attachedCoords = _transform.GetMapCoordinates(attachedEnt);
        var newPos = attachedCoords.Position;
        emitter.MapCoords = attachedCoords;

        if (!emitter.VelocityInitialized)
        {
            emitter.PreviousPosition = newPos;
            emitter.VelocityInitialized = true;
            return;
        }

        var delta = newPos - emitter.PreviousPosition;
        if (dt > 0f)
        {
            // Detect teleports, without this, particles inheriting velocity get sent flying off-screen
            // when the attached entity is moved (admin teleport, jaunt, bluespace, etc etc)
            emitter.EmitterVelocity = delta.LengthSquared() > TeleportThresholdSquared
                ? Vector2.Zero
                : delta / dt;
        }
        emitter.PreviousPosition = newPos;
    }

    private static void AdvanceEmitterAge(ActiveEmitter emitter, TimeSpan ageCheck)
    {
        emitter.Age += ageCheck;

        if (emitter.Exhausted)
            return;

        var ovr = emitter.Overrides;
        var duration = (float)(ovr?.Duration ?? emitter.Proto.Duration).TotalSeconds;
        if (duration > 0f && emitter.Age.TotalSeconds >= duration)
            emitter.Exhausted = true;
    }

    private void AgeOffScreenParticles(ActiveEmitter emitter, TimeSpan ageCheck)
    {
        foreach (var p in emitter.Particles)
        {
            if (!p.Alive)
                continue;
            p.Age += ageCheck;
            if (p.Age >= p.Lifetime)
            {
                p.Alive = false;
                _liveParticleCount--;
                emitter.FreePool.Enqueue(p);
            }
        }
    }

    #endregion

    #region Emitter Internals

    // <summary>
    // Creates a new ActiveEmitter from a prototype and initial state.
    // </summary>
    private ActiveEmitter CreateEmitter(ParticleEffectPrototype proto, MapCoordinates coords, EntityUid? attached)
    {
        var emitter = new ActiveEmitter
        {
            Proto = proto,
            MapCoords = coords,
            AttachedEntity = attached,
            Handle = _nextHandle++,
        };
        ResolveFrames(emitter);

        emitter.EffectiveEmitAngle = (float) emitter.Proto.EmitAngle.Theta;

        foreach (var _ in proto.Bursts)
            emitter.FiredBursts.Add(false);

        return emitter;
    }

    /// <summary>
    /// Stops a running emitter, preventing new particles from being emitted. Existing particles live out their lifetime.
    /// </summary>
    public void StopEffect(uint handle)
    {
        if (handle == 0)
            return;
        foreach (var emitter in _emitters)
        {
            if (emitter.Handle == handle)
            {
                emitter.Exhausted = true;
                break;
            }
        }
    }

    /// <summary>
    /// Stops a running emitter by direct reference. Existing particles live out their lifetime.
    /// </summary>
    public static void StopEffect(ActiveEmitter emitter) => emitter.Exhausted = true;

    /// <summary>
    /// Updates the intensity multiplier on a running emitter by handle.
    /// </summary>
    public void UpdateIntensity(uint handle, float intensity)
    {
        if (handle == 0)
            return;
        foreach (var emitter in _emitters)
        {
            if (emitter.Handle == handle)
            {
                emitter.Intensity = intensity;
                break;
            }
        }
    }

    /// <summary>
    /// Updates the intensity multiplier on a running emitter by direct reference.
    /// </summary>
    public static void UpdateIntensity(ActiveEmitter emitter, float intensity) => emitter.Intensity = intensity;

    private void TickEmitter(ActiveEmitter emitter, float dt, float eyeAngle)
    {
        var proto = emitter.Proto;

        // Aim-at: recompute emit angle toward target each tick
        Vector2? targetWorldPos = null;
        if (emitter.TargetEntity is { } targetEnt)
        {
            if (!Deleted(targetEnt))
                targetWorldPos = _transform.GetMapCoordinates(targetEnt).Position;
            else
                emitter.TargetEntity = null; // entity GONE, fall to TargetPosition
        }

        targetWorldPos ??= emitter.TargetPosition;
        if (targetWorldPos.HasValue)
        {
            var worldDir = targetWorldPos.Value - emitter.MapCoords.Position;
            if (worldDir.LengthSquared() > 0.0001f)
            {
                // Convert world direction to screen-space direction to angle (0 = screen-up)
                var cosE = MathF.Cos(eyeAngle);
                var sinE = MathF.Sin(eyeAngle);
                var sx = worldDir.X * cosE - worldDir.Y * sinE;
                var sy = worldDir.X * sinE + worldDir.Y * cosE;
                emitter.EffectiveEmitAngle = MathF.Atan2(sx, sy);
            }
        }
        else
        {
            // No target, keep in sync with the overridden emit angle if set, otherwise prototype default
            var baseAngle = emitter.Overrides?.EmitAngle ?? emitter.Proto.EmitAngle;
            emitter.EffectiveEmitAngle = (float)baseAngle.Theta;
        }

        // Resolve overridable scalars once per tick
        var ovr = emitter.Overrides;
        var drag = ovr?.Drag ?? proto.Drag;
        var constForce = ovr?.ConstantForce ?? proto.ConstantForce;
        var termSpeed = ovr?.TerminalSpeed ?? proto.TerminalSpeed;
        var gravity = ovr?.Gravity ?? proto.Gravity;
        var noiseStr = ovr?.NoiseStrength ?? proto.NoiseStrength;
        var noiseFreq = ovr?.NoiseFrequency ?? proto.NoiseFrequency;
        var duration = (float)(ovr?.Duration ?? proto.Duration).TotalSeconds;
        var emissionRate = ovr?.EmissionRate ?? proto.EmissionRate;
        var maxCount = ovr?.MaxCount ?? proto.MaxCount;

        // Drag is exp(-drag*dt) per tick, same value for every particle. Do the math once and pass
        // the multiplier in to not do this math every single particle..
        var dragMul = drag > 0f ? MathF.Exp(-drag * dt) : 1f;
        var termSpeedSq = termSpeed > 0f ? termSpeed * termSpeed : 0f;

        // RSI animation
        if (emitter.Delays.Length > 0 && emitter.Frames.Length > 0)
        {
            emitter.AnimTimer += dt;
            while (emitter.AnimTimer >= emitter.Delays[emitter.AnimFrame])
            {
                var delay = emitter.Delays[emitter.AnimFrame];
                if (delay <= 0f)
                    break;
                emitter.AnimTimer -= delay;
                emitter.AnimFrame = (emitter.AnimFrame + 1) % emitter.Frames.Length;
            }
        }

        // Simulate live particles. Death decrements _liveParticleCount so the emission caps below
        // (which read _globalBudget - _liveParticleCount) see the freed-up capacity immediately.
        // liveCount is the post-tick number of survivors used to enforce MaxCount below.
        var liveCount = 0;
        var dtSpan = TimeSpan.FromSeconds(dt);
        foreach (var p in emitter.Particles)
        {
            if (!p.Alive)
                continue;

            p.Age += dtSpan;

            if (p.Age >= p.Lifetime)
            {
                if (proto.SubEmitterOnDeath.HasValue)
                {
                    var worldPos = ComputeParticleWorldPos(p, emitter, eyeAngle);
                    _pendingSubEmitters.Add((proto.SubEmitterOnDeath.Value,
                        new MapCoordinates(worldPos, emitter.MapCoords.MapId)));
                }

                p.Alive = false;
                _liveParticleCount--;
                emitter.FreePool.Enqueue(p);
                continue;
            }

            SimulateParticle(p, dt, dragMul, constForce, termSpeedSq, termSpeed, gravity, noiseStr, noiseFreq, proto);
            liveCount++;
        }

        // Timed bursts
        if (emitter.Exhausted)
            return;

        var qualityMult = proto.IgnoreQualitySettings ? 1f : QualityMultipliers[Math.Clamp(_quality, 0, QualityMultipliers.Length - 1)];

        for (var b = 0; b < proto.Bursts.Count; b++)
        {
            if (emitter.FiredBursts[b])
                continue;
            var burst = proto.Bursts[b];
            if (emitter.Age < burst.Time)
                continue;

            var toEmit = (int) Math.Ceiling(burst.Count * qualityMult * emitter.Intensity);
            for (var j = 0; j < toEmit && _liveParticleCount < _globalBudget; j++)
                EmitParticle(emitter, eyeAngle);
            emitter.FiredBursts[b] = true;
        }

        // Continuous emission
        // IgnoreQualitySettings emitters are capped at IgnoreQualityMaxParticles unless quality is High.
        var effectiveMax = proto.IgnoreQualitySettings && _quality < 3
            ? Math.Min(maxCount, IgnoreQualityMaxParticles)
            : maxCount;
        var scaledMax = (int) Math.Ceiling(Math.Min(effectiveMax, HardMaxParticles) * qualityMult * emitter.Intensity);
        var capacity = _globalBudget - _liveParticleCount;
        var canEmit = Math.Min(scaledMax - liveCount, capacity);
        if (canEmit > 0)
        {
            var emissionMult = 1f;
            if (proto.EmissionOverTime.Count > 0)
            {
                var t = duration > 0f
                    ? Math.Clamp((float) (emitter.Age.TotalSeconds / duration), 0f, 1f)
                    : Math.Clamp((float) emitter.Age.TotalSeconds, 0f, 1f);
                emissionMult = SampleCurve(proto.EmissionOverTime, t);
            }

            emitter.EmitAccum += emissionRate * emissionMult * dt * emitter.Intensity;
            var toEmit = (int) emitter.EmitAccum;
            emitter.EmitAccum -= toEmit;
            toEmit = Math.Min(toEmit, canEmit);

            for (var i = 0; i < toEmit; i++)
                EmitParticle(emitter, eyeAngle);
        }
    }

    private void BurstEmit(ActiveEmitter emitter)
    {
        var proto = emitter.Proto;
        var eyeAngle = (float) _eye.CurrentEye.Rotation;
        // Bypass quality settings for gameplay-critical particles
        var qualityMult = proto.IgnoreQualitySettings ? 1f : QualityMultipliers[Math.Clamp(_quality, 0, QualityMultipliers.Length - 1)];
        // IgnoreQualitySettings emitters are capped at IgnoreQualityMaxParticles to prevent performance issues or otherwise abuse.
        var effectiveMax = proto.IgnoreQualitySettings && _quality < 3
            ? Math.Min(proto.MaxCount, IgnoreQualityMaxParticles)
            : proto.MaxCount;
        var count = (int) Math.Ceiling(Math.Min(effectiveMax, HardMaxParticles) * qualityMult * emitter.Intensity);
        var capacity = _globalBudget - _liveParticleCount;
        if (capacity <= 0)
            return;
        if (count > capacity)
            count = capacity;
        for (int i = 0; i < count; i++)
            EmitParticle(emitter, eyeAngle);
    }

    private void EmitParticle(ActiveEmitter emitter, float eyeAngle)
    {
        var proto = emitter.Proto;

        ParticleData p;
        bool recycled;
        if (emitter.FreePool.TryDequeue(out var pooled))
        {
            p = pooled;
            p.Reset();
            recycled = true;
        }
        else
        {
            p = new ParticleData();
            recycled = false;
        }

        p.Alive = true;
        _liveParticleCount++;

        // Resolve spawn time overridable fields
        var ovr = emitter.Overrides;
        var lifetime = (float) (ovr?.Lifetime ?? proto.Lifetime).TotalSeconds;
        var lifetimeVar = (float) (ovr?.LifetimeVariance ?? proto.LifetimeVariance).TotalSeconds;
        var spreadAngle = (float) (ovr?.SpreadAngle?.Theta ?? proto.SpreadAngle.Theta);
        var speed0 = ovr?.Speed ?? proto.Speed;
        var speedVar = ovr?.SpeedVariance ?? proto.SpeedVariance;
        var sizeVar = ovr?.SizeVariance ?? proto.SizeVariance;
        var inheritVel = ovr?.InheritVelocity ?? proto.InheritVelocity;
        var startRot = (float) (ovr?.StartRotation?.Theta ?? proto.StartRotation.Theta);
        var startRotVar = (float) (ovr?.StartRotationVariance?.Theta ?? proto.StartRotationVariance.Theta);
        var rotSpeed = (float) (ovr?.RotationSpeed?.Theta ?? proto.RotationSpeed.Theta);
        var rotSpeedVar = (float) (ovr?.RotationSpeedVariance?.Theta ?? proto.RotationSpeedVariance.Theta);

        p.Lifetime = TimeSpan.FromSeconds(lifetime + _random.NextFloat(-lifetimeVar, lifetimeVar));
        if (p.Lifetime < TimeSpan.FromSeconds(MinParticleLifetimeSeconds))
            p.Lifetime = TimeSpan.FromSeconds(MinParticleLifetimeSeconds);

        var spreadHalf = spreadAngle * 0.5f;
        var angle = emitter.EffectiveEmitAngle + _random.NextFloat(-spreadHalf, spreadHalf);

        var speed = speed0 + _random.NextFloat(-speedVar, speedVar);
        speed = Math.Max(speed, 0f);

        p.Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * speed;
        p.LocalOffset = SampleEmissionShape(proto.Shape);

        // InheritVelocity: convert emitter world velocity to screen space then add
        if (inheritVel != 0f && emitter.EmitterVelocity != Vector2.Zero)
        {
            var cosE = MathF.Cos(eyeAngle);
            var sinE = MathF.Sin(eyeAngle);
            var wv = emitter.EmitterVelocity * inheritVel;
            var screenVel = new Vector2(wv.X * cosE - wv.Y * sinE, wv.X * sinE + wv.Y * cosE);
            p.Velocity += screenVel;
        }

        if (proto.WorldSpace)
            p.SpawnOrigin = emitter.MapCoords.Position;

        p.SpawnSpeed = speed;
        p.SpawnIntensity = emitter.Intensity;

        // SizeVariance
        if (sizeVar > 0f)
            p.SizeMultiplier = 1f + _random.NextFloat(-sizeVar, sizeVar);
        else
            p.SizeMultiplier = 1f;

        p.Rotation = startRot + _random.NextFloat(-startRotVar, startRotVar);
        p.RotationSpeed = rotSpeed + _random.NextFloat(-rotSpeedVar, rotSpeedVar);

        // Unique noise offset so each particle gets different turbulence
        p.NoiseOffset = new Vector2(_random.NextFloat(-100f, 100f), _random.NextFloat(-100f, 100f));

        if (!recycled)
            emitter.Particles.Add(p);

        // Sub-emitter on spawn
        if (proto.SubEmitterOnSpawn.HasValue)
        {
            var worldPos = ComputeParticleWorldPos(p, emitter, eyeAngle);
            _pendingSubEmitters.Add((proto.SubEmitterOnSpawn.Value,
                new MapCoordinates(worldPos, emitter.MapCoords.MapId)));
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Advances a single live particle's simulation by one step.
    /// </summary>
    private static void SimulateParticle(
        ParticleData p,
        float dt,
        float dragMul,
        Vector2 constForce,
        float termSpeedSq,
        float termSpeed,
        float gravity,
        float noiseStr,
        float noiseFreq,
        ParticleEffectPrototype proto)
    {
        // Drag
        if (dragMul != 1f)
            p.Velocity *= dragMul;

        // ConstantForce
        if (constForce != Vector2.Zero)
            p.Velocity += constForce * dt;

        // ForceOverLifetime
        if (proto.ForceOverLifetime.Count > 0)
            p.Velocity += SampleVector2Curve(proto.ForceOverLifetime, p.AgeRatio) * dt;

        // SpeedOverLifetime: rescale velocity magnitude to the curve-defined speed
        if (proto.SpeedOverLifetime.Count > 0)
        {
            var curveSpeed = SampleCurve(proto.SpeedOverLifetime, p.AgeRatio) * p.SpawnSpeed;
            var currentSpeed = p.Velocity.Length();
            if (currentSpeed > 0f)
                p.Velocity = p.Velocity / currentSpeed * curveSpeed;
        }

        // Terminal speed cap
        if (termSpeedSq > 0f)
        {
            var speedSq = p.Velocity.LengthSquared();
            if (speedSq > termSpeedSq)
                p.Velocity *= termSpeed / MathF.Sqrt(speedSq);
        }

        // Advance position
        p.LocalOffset += p.Velocity * dt;

        // VelocityOverLifetime: positional nudge (does not modify velocity)
        if (proto.VelocityOverLifetime.Count > 0)
            p.LocalOffset += SampleVector2Curve(proto.VelocityOverLifetime, p.AgeRatio) * dt;

        // Gravity
        if (gravity != 0f)
            p.LocalOffset.Y += -gravity * dt * p.AgeRatio;

        // Noise
        if (noiseStr > 0f)
        {
            var noiseT = (float) p.Age.TotalSeconds * noiseFreq;
            var nx = ValueNoise(p.NoiseOffset.X + noiseT, p.NoiseOffset.Y);
            var ny = ValueNoise(p.NoiseOffset.X, p.NoiseOffset.Y + noiseT);
            p.LocalOffset += new Vector2(nx, ny) * noiseStr * dt;
        }

        // SPIN!!!!
        if (p.RotationSpeed != 0f)
            p.Rotation += p.RotationSpeed * dt;
    }

    /// <summary>
    /// Converts a particle's screen-space LocalOffset to a world position.
    /// </summary>
    private static Vector2 ComputeParticleWorldPos(ParticleData p, ActiveEmitter emitter, float eyeAngle)
    {
        var cosR = MathF.Cos(-eyeAngle);
        var sinR = MathF.Sin(-eyeAngle);
        var worldOffset = new Vector2(p.LocalOffset.X * cosR - p.LocalOffset.Y * sinR,
                                      p.LocalOffset.X * sinR + p.LocalOffset.Y * cosR);
        var origin = emitter.Proto.WorldSpace ? p.SpawnOrigin : emitter.MapCoords.Position;
        return origin + worldOffset;
    }

    private void ResolveFrames(ActiveEmitter emitter)
    {
        var proto = emitter.Proto;

        if (_frameCache.TryGetValue(proto, out var cached))
        {
            emitter.Frames = cached.Frames;
            emitter.Delays = cached.Delays;
            return;
        }

        if (_frameResolveFailures.Contains(proto))
            return;

        Texture[]? frames = null;
        float[] delays = Array.Empty<float>();

        switch (proto.Sprite)
        {
            case SpriteSpecifier.Rsi rsi:
                {
                    RSI? resource;
                    try
                    {
                        var path = rsi.RsiPath.IsRooted
                            ? rsi.RsiPath
                            : SpriteSpecifierSerializer.TextureRoot / rsi.RsiPath;
                        resource = _resource.GetResource<RSIResource>(path).RSI;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Could not resolve RSI resource '{rsi.RsiPath}' for particle prototype {proto.ID}: {e}");
                        _frameResolveFailures.Add(proto);
                        return;
                    }

                    if (!resource.TryGetState(rsi.RsiState, out var state))
                    {
                        _frameResolveFailures.Add(proto);
                        return;
                    }

                    frames = state.GetFrames(RsiDirection.South);
                    delays = state.GetDelays();
                    break;
                }
            case SpriteSpecifier.Texture tex:
                {
                    try
                    {
                        frames = new[] { _sprite.Frame0(tex) };
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Could not resolve sprite texture '{tex.TexturePath}' for particle prototype {proto.ID}: {e}");
                        _frameResolveFailures.Add(proto);
                        return;
                    }
                    break;
                }
        }

        if (frames == null)
        {
            _frameResolveFailures.Add(proto);
            return;
        }

        _frameCache[proto] = (frames, delays);
        emitter.Frames = frames;
        emitter.Delays = delays;
    }

    private Vector2 SampleEmissionShape(EmissionShapeData shape)
    {
        switch (shape.Type)
        {
            case EmissionShapeType.Point:
                return Vector2.Zero;
            case EmissionShapeType.CircleEdge:
                {
                    var a = _random.NextFloat(0f, MathF.PI * 2f);
                    return new Vector2(MathF.Cos(a), MathF.Sin(a)) * shape.Radius;
                }
            case EmissionShapeType.CircleFill:
                {
                    var a = _random.NextFloat(0f, MathF.PI * 2f);
                    var r = shape.Radius * MathF.Sqrt(_random.NextFloat(0f, 1f));
                    return new Vector2(MathF.Cos(a), MathF.Sin(a)) * r;
                }
            case EmissionShapeType.Box:
                {
                    return new Vector2(_random.NextFloat(-shape.BoxExtents.X, shape.BoxExtents.X),
                                       _random.NextFloat(-shape.BoxExtents.Y, shape.BoxExtents.Y));
                }
            default:
                return Vector2.Zero;
        }
    }

    #endregion

    #region Curve Samplers

    // math scares me
    public static float SampleCurve(List<ParticleCurveKey> curve, float t)
    {
        if (curve.Count == 0)
            return 1f;
        if (curve.Count == 1)
            return curve[0].Value;

        ParticleCurveKey? prev = null, next = null;
        foreach (var key in curve)
        {
            if (key.Time <= t)
                prev = key;
            else
            {
                next = key;
                break;
            }
        }
        if (prev == null)
            return curve[0].Value;
        if (next == null)
            return prev.Value.Value;

        var span = next.Value.Time - prev.Value.Time;
        if (span <= 0f)
            return prev.Value.Value;
        return prev.Value.Value + (next.Value.Value - prev.Value.Value) * ((t - prev.Value.Time) / span);
    }

    public static Color SampleColorCurve(List<ColorCurveKey> curve, float t)
    {
        if (curve.Count == 0)
            return Color.White;
        if (curve.Count == 1)
            return curve[0].Color;

        ColorCurveKey? prev = null, next = null;
        foreach (var key in curve)
        {
            if (key.Time <= t)
                prev = key;
            else
            {
                next = key;
                break;
            }
        }
        if (prev == null)
            return curve[0].Color;
        if (next == null)
            return prev.Value.Color;

        var span = next.Value.Time - prev.Value.Time;
        if (span <= 0f)
            return prev.Value.Color;
        return Color.InterpolateBetween(prev.Value.Color, next.Value.Color, (t - prev.Value.Time) / span);
    }

    public static Vector2 SampleVector2Curve(List<Vector2CurveKey> curve, float t)
    {
        if (curve.Count == 0)
            return Vector2.Zero;
        if (curve.Count == 1)
            return curve[0].Value;

        Vector2CurveKey? prev = null, next = null;
        foreach (var key in curve)
        {
            if (key.Time <= t)
                prev = key;
            else
            {
                next = key;
                break;
            }
        }
        if (prev == null)
            return curve[0].Value;
        if (next == null)
            return prev.Value.Value;

        var span = next.Value.Time - prev.Value.Time;
        if (span <= 0f)
            return prev.Value.Value;
        return Vector2.Lerp(prev.Value.Value, next.Value.Value, (t - prev.Value.Time) / span);
    }

    #endregion

    #region Value Noise

    /// <summary>
    /// A simple 2D value noise function for particle turbulence. Not Perlin or Simplex, just a grid of random values with smooth interpolation.
    /// </summary>
    private static float ValueNoise(float x, float y)
    {
        var ix = (int) MathF.Floor(x);
        var iy = (int) MathF.Floor(y);
        var fx = x - ix;
        var fy = y - iy;

        // Smooth interpolation
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);

        var a = Hash(ix, iy);
        var b = Hash(ix + 1, iy);
        var c = Hash(ix, iy + 1);
        var d = Hash(ix + 1, iy + 1);

        // maths scare me what do these letters mean
        return a + (b - a) * fx + (c - a) * fy + (d - b - c + a) * fx * fy;
    }

    // random bullshit go
    private static float Hash(int x, int y)
    {
        var n = x + y * 57;
        n = (n << 13) ^ n;
        return 1f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f;
    }

    #endregion
}
