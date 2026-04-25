// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Particles;

/// <summary>
/// Draws all live particles for every active emitter each frame.
/// </summary>
public sealed class ParticleOverlay : Overlay
{
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly ParticleSystem _system;
    private readonly Dictionary<string, ShaderInstance> _shaderCache = new();

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public ParticleOverlay(ParticleSystem system)
    {
        IoCManager.InjectDependencies(this);
        _system = system;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var mapId = args.MapId;
        var eyeAngle = (float)_eye.CurrentEye.Rotation;
        var cosR = MathF.Cos(-eyeAngle);
        var sinR = MathF.Sin(-eyeAngle);

        // Sort emitters by RenderLayer so lower layers draw first
        var sorted = _system.GetEmitters().OrderBy(e => e.Overrides?.RenderLayer ?? e.Proto.RenderLayer);

        string? activeShader = null; // track to avoid redundant calls

        foreach (var emitter in sorted)
        {
            if (emitter.MapCoords.MapId != mapId) continue;
            if (!args.WorldBounds.Contains(emitter.MapCoords.Position)) continue;
            if (emitter.Frames.Length == 0) continue;

            var proto = emitter.Proto;
            var ovr = emitter.Overrides;
            var tex = emitter.Frames[emitter.AnimFrame];
            var baseHalfSize = (ovr?.ParticleSize ?? proto.ParticleSize) * 0.5f;

            // Resolve shader override takes precedence, then prototype, then null
            string? wantedShader = ovr?.Shader ?? (string.IsNullOrEmpty(proto.Shader) ? null : proto.Shader);

            if (wantedShader != activeShader)
            {
                if (wantedShader != null)
                {
                    if (!_shaderCache.TryGetValue(wantedShader, out var cached))
                    {
                        if (_proto.Resolve<ShaderPrototype>(wantedShader, out var shaderProto))
                        {
                            cached = shaderProto.Instance();
                            _shaderCache[wantedShader] = cached;
                        }
                    }
                    handle.UseShader(cached);
                }
                else
                {
                    handle.UseShader(null);
                }
                activeShader = wantedShader;
            }

            var screenOrigin = emitter.MapCoords.Position;

            foreach (var particle in emitter.Particles)
            {
                if (!particle.Alive) continue;

                var t = particle.AgeRatio;

                // Color: use ColorOverLifetime gradient if available, otherwise lerp StartColor to EndColor
                Color color;
                if (proto.ColorOverLifetime.Count > 0)
                    color = ParticleSystem.SampleColorCurve(proto.ColorOverLifetime, t);
                else
                {
                    var startColor = ovr?.StartColor ?? proto.StartColor;
                    var endColor   = ovr?.EndColor   ?? proto.EndColor;
                    color = Color.InterpolateBetween(startColor, endColor, t);
                }

                // ColorOverride tint
                var tintColor = ovr?.ColorOverride ?? emitter.ColorOverride;
                if (tintColor is { } tint)
                    color = new Color(color.R * tint.R, color.G * tint.G, color.B * tint.B, color.A * tint.A);

                // AlphaOverLifetime: multiplied on top of color alpha
                if (proto.AlphaOverLifetime.Count > 0)
                {
                    var alpha = ParticleSystem.SampleCurve(proto.AlphaOverLifetime, t);
                    color = color.WithAlpha(color.A * alpha);
                }

                // Size: base * intensity * SizeMultiplier * SizeOverLifetime curve
                var halfSize = baseHalfSize * particle.SpawnIntensity * particle.SizeMultiplier;
                if (proto.SizeOverLifetime.Count > 0)
                    halfSize *= ParticleSystem.SampleCurve(proto.SizeOverLifetime, t);

                // Convert screen-space LocalOffset to world offset
                var local = particle.LocalOffset;
                var worldOffset = new Vector2(local.X * cosR - local.Y * sinR,
                                              local.X * sinR + local.Y * cosR);

                var origin = proto.WorldSpace ? particle.SpawnOrigin : screenOrigin;
                var worldPos = origin + worldOffset;

                // StretchFactor: elongate along velocity direction proportional to speed
                var stretchFactor = ovr?.StretchFactor ?? proto.StretchFactor;
                if (stretchFactor > 0f)
                {
                    var velLen = particle.Velocity.Length();
                    if (velLen > 0.001f)
                    {
                        var stretchY = 1f + velLen * stretchFactor;
                        // Align sprite "up" with velocity direction (screen-space atan2)
                        var velAngle = MathF.Atan2(particle.Velocity.X, particle.Velocity.Y);
                        var totalRot = -eyeAngle + velAngle;
                        var cV = MathF.Cos(totalRot);
                        var sV = MathF.Sin(totalRot);
                        handle.SetTransform(new Matrix3x2(cV, sV, -sV, cV, worldPos.X, worldPos.Y));
                        handle.DrawTextureRect(tex,
                            new Box2(-halfSize, -halfSize * stretchY, halfSize, halfSize * stretchY),
                            color);
                        continue; // skip
                    }
                }

                // Draw with rotation applied. Rotation is in radians, positive is clockwise, and 0 means "facing up" (aligned with SCREEN/eye/whatever Y axis).
                var totalRotation = -eyeAngle + particle.Rotation;
                var cos = MathF.Cos(totalRotation);
                var sin = MathF.Sin(totalRotation);
                handle.SetTransform(new Matrix3x2(cos, sin, -sin, cos, worldPos.X, worldPos.Y));
                handle.DrawTextureRect(tex, new Box2(-halfSize, -halfSize, halfSize, halfSize), color);
            }
        }

        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(null);
    }
}
