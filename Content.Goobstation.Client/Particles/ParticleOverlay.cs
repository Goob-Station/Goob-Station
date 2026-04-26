// SPDX-License-Identifier: AGPL-3.0-or-later

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
    private readonly Dictionary<string, ShaderInstance?> _shaderCache = new();
    private readonly List<ActiveEmitter> _sortBuffer = new();
    private static readonly Comparison<ActiveEmitter> RenderLayerComparison =
        (a, b) => (a.Overrides?.RenderLayer ?? a.Proto.RenderLayer)
            .CompareTo(b.Overrides?.RenderLayer ?? b.Proto.RenderLayer);

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    // Engine cap on quads per <c>DrawPrimitives</c> call, we never want to feed more than this in a single submit.
    private const int MaxQuadsPerDraw = 16383;

    private static readonly Vector2 Uv2BL = new(0f, 0f);
    private static readonly Vector2 Uv2BR = new(1f, 0f);
    private static readonly Vector2 Uv2TR = new(1f, 1f);
    private static readonly Vector2 Uv2TL = new(0f, 1f);

    private readonly DrawVertexUV2DColor[] _vertexScratch = new DrawVertexUV2DColor[MaxQuadsPerDraw * 4];
    private readonly ushort[] _indexScratch;

    public ParticleOverlay(ParticleSystem system)
    {
        IoCManager.InjectDependencies(this);
        _system = system;
        _indexScratch = BuildQuadIndices(MaxQuadsPerDraw);
    }

    private static ushort[] BuildQuadIndices(int quadCount)
    {
        var indices = new ushort[quadCount * 6];
        for (var q = 0; q < quadCount; q++)
        {
            var v = (ushort)(q * 4);
            var i = q * 6;
            indices[i + 0] = v;
            indices[i + 1] = (ushort)(v + 1);
            indices[i + 2] = (ushort)(v + 2);
            indices[i + 3] = v;
            indices[i + 4] = (ushort)(v + 2);
            indices[i + 5] = (ushort)(v + 3);
        }
        return indices;
    }


    /// <c>DrawPrimitives</c> rejects atlased textures, so we gotta resolve them manually here to get the correct UVs.
    private static (Texture src, Box2 uv) ResolveAtlas(Texture tex)
    {
        if (tex is AtlasTexture atlas)
        {
            var w = (float) atlas.SourceTexture.Width;
            var h = (float) atlas.SourceTexture.Height;
            var sr = atlas.SubRegion;
            return (atlas.SourceTexture,
                new Box2(sr.Left / w, (h - sr.Bottom) / h, sr.Right / w, (h - sr.Top) / h));
        }
        return (tex, new Box2(0f, 0f, 1f, 1f));
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var mapId = args.MapId;
        var eyeAngle = (float)_eye.CurrentEye.Rotation;
        var cosR = MathF.Cos(-eyeAngle);
        var sinR = MathF.Sin(-eyeAngle);

        _sortBuffer.Clear();
        var live = _system.GetEmitters();
        for (var i = 0; i < live.Count; i++)
            _sortBuffer.Add(live[i]);
        _sortBuffer.Sort(RenderLayerComparison);

        string? activeShader = null;

        foreach (var emitter in _sortBuffer)
        {
            if (emitter.MapCoords.MapId != mapId) continue;
            if (!args.WorldBounds.Contains(emitter.MapCoords.Position)) continue;
            if (emitter.Frames.Length == 0) continue;

            var proto = emitter.Proto;
            var ovr = emitter.Overrides;
            var rawTex = emitter.Frames[emitter.AnimFrame];
            var (drawTex, uv) = ResolveAtlas(rawTex);
            var baseHalfSize = (ovr?.ParticleSize ?? proto.ParticleSize) * 0.5f;

            var wantedShader = ovr?.Shader ?? (string.IsNullOrEmpty(proto.Shader) ? null : proto.Shader);
            if (wantedShader != activeShader)
            {
                if (wantedShader != null)
                {
                    if (!_shaderCache.TryGetValue(wantedShader, out var cached))
                    {
                        if (_proto.Resolve<ShaderPrototype>(wantedShader, out var shaderProto))
                            cached = shaderProto.Instance();
                        _shaderCache[wantedShader] = cached;
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
            var stretchFactor = ovr?.StretchFactor ?? proto.StretchFactor;
            var startColor = ovr?.StartColor ?? proto.StartColor;
            var endColor = ovr?.EndColor ?? proto.EndColor;
            var tintColor = ovr?.ColorOverride ?? emitter.ColorOverride;

            var hasColorCurve = proto.ColorOverLifetime.Count > 0;
            var hasAlphaCurve = proto.AlphaOverLifetime.Count > 0;
            var hasSizeCurve = proto.SizeOverLifetime.Count > 0;

            var uvBL = uv.BottomLeft;
            var uvBR = uv.BottomRight;
            var uvTR = uv.TopRight;
            var uvTL = uv.TopLeft;

            var verts = _vertexScratch.AsSpan();
            var quadCount = 0;

            foreach (var particle in emitter.Particles)
            {
                if (!particle.Alive) continue;

                if (quadCount >= MaxQuadsPerDraw)
                {
                    handle.DrawPrimitives(
                        DrawPrimitiveTopology.TriangleList,
                        drawTex,
                        _indexScratch.AsSpan(0, quadCount * 6),
                        verts.Slice(0, quadCount * 4));
                    quadCount = 0;
                }

                var t = particle.AgeRatio;

                Color color;
                if (hasColorCurve)
                    color = ParticleSystem.SampleColorCurve(proto.ColorOverLifetime, t);
                else
                    color = Color.InterpolateBetween(startColor, endColor, t);

                if (tintColor is { } tint)
                    color = new Color(color.R * tint.R, color.G * tint.G, color.B * tint.B, color.A * tint.A);

                if (hasAlphaCurve)
                {
                    var alpha = ParticleSystem.SampleCurve(proto.AlphaOverLifetime, t);
                    color = color.WithAlpha(color.A * alpha);
                }

                var halfSize = baseHalfSize * particle.SpawnIntensity * particle.SizeMultiplier;
                if (hasSizeCurve)
                    halfSize *= ParticleSystem.SampleCurve(proto.SizeOverLifetime, t);

                var local = particle.LocalOffset;
                var worldOffset = new Vector2(local.X * cosR - local.Y * sinR,
                                              local.X * sinR + local.Y * cosR);
                var origin = proto.WorldSpace ? particle.SpawnOrigin : screenOrigin;
                var worldPos = origin + worldOffset;

                float halfX = halfSize;
                float halfY = halfSize;
                float cos, sin;
                var velSq = particle.Velocity.LengthSquared();
                if (stretchFactor > 0f && velSq > 0.000001f)
                {
                    var velLen = MathF.Sqrt(velSq);
                    halfY = halfSize * (1f + velLen * stretchFactor);
                    var invLen = 1f / velLen;
                    var cosVel = particle.Velocity.Y * invLen;
                    var sinVel = particle.Velocity.X * invLen;
                    cos = cosR * cosVel - sinR * sinVel;
                    sin = sinR * cosVel + cosR * sinVel;
                }
                else
                {
                    var totalRot = -eyeAngle + particle.Rotation;
                    cos = MathF.Cos(totalRot);
                    sin = MathF.Sin(totalRot);
                }

                var hxC = halfX * cos;
                var hxS = halfX * sin;
                var hyC = halfY * cos;
                var hyS = halfY * sin;
                var tx = worldPos.X;
                var ty = worldPos.Y;

                var blX = -hxC + hyS + tx;
                var blY = -hxS - hyC + ty;
                var brX = hxC + hyS + tx;
                var brY = hxS - hyC + ty;
                var trX = hxC - hyS + tx;
                var trY = hxS + hyC + ty;
                var tlX = -hxC - hyS + tx;
                var tlY = -hxS + hyC + ty;

                var v = quadCount * 4;
                verts[v + 0] = new DrawVertexUV2DColor(new Vector2(blX, blY), uvBL, color) { UV2 = Uv2BL };
                verts[v + 1] = new DrawVertexUV2DColor(new Vector2(brX, brY), uvBR, color) { UV2 = Uv2BR };
                verts[v + 2] = new DrawVertexUV2DColor(new Vector2(trX, trY), uvTR, color) { UV2 = Uv2TR };
                verts[v + 3] = new DrawVertexUV2DColor(new Vector2(tlX, tlY), uvTL, color) { UV2 = Uv2TL };

                quadCount++;
            }

            if (quadCount == 0)
                continue;

            handle.DrawPrimitives(
                DrawPrimitiveTopology.TriangleList,
                drawTex,
                _indexScratch.AsSpan(0, quadCount * 6),
                verts.Slice(0, quadCount * 4));
        }

        _sortBuffer.Clear();
        handle.UseShader(null);
    }
}
