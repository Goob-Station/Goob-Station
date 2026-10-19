// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Timing;
using Robust.Shared.Enums;

namespace Content.Client.MalfAI.Overlays;

/// <summary>
/// Renders a screen-space expanding cyan circle, synced to the server timeline and centered at the AI origin.
/// </summary>
public sealed class MalfAiDoomsdayRippleOverlay : Overlay
{
    private readonly MalfAiDoomsdayRippleClientSystem _system;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    // Tunables for look & feel.
    private const float RingThicknessPixels = 48f; // was 36f, now thicker
    private static readonly Color CyanGlow = Color.Cyan; // explicit ring color

    public MalfAiDoomsdayRippleOverlay(MalfAiDoomsdayRippleClientSystem system)
    {
        _system = system;
        ZIndex = 101;
    }

    private bool _shouldDraw;
    private Vector2 _centerWorld;
    private float _radiusTiles;
    private float _ringThicknessTiles;
    private float _ringAlpha;

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        _shouldDraw = false;
        if (!_system.IsActive)
            return false;

        var now = IoCManager.Resolve<IGameTiming>().CurTime.TotalSeconds;
        var elapsed = Math.Max(0, now - _system.ServerStartSeconds);
        if (_system.Duration <= TimeSpan.Zero)
            return false;

        var t = Math.Clamp((float)(elapsed / _system.Duration.TotalSeconds), 0f, 1f);
        _radiusTiles = t * _system.MaxRadiusTiles;
        if (_radiusTiles <= 0f)
            return false;

        _centerWorld = _system.OriginWorld;
        _ringThicknessTiles = RingThicknessPixels / 32f;

        // Simple alpha fade in and out
        var tIn = Math.Clamp(t * 3f, 0f, 1f);
        var easeIn = tIn * tIn * (3f - 2f * tIn);
        var tOut = Math.Clamp((t - 0.8f) / 0.2f, 0f, 1f);
        var easeOut = tOut * tOut * (3f - 2f * tOut);
        _ringAlpha = easeIn * (1f - easeOut);
        if (_system.CenterFlash && elapsed < 0.2f)
            _ringAlpha = MathF.Min(1f, _ringAlpha + 0.5f);

        _shouldDraw = true;
        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_shouldDraw)
            return;

        var worldHandle = args.WorldHandle;
        if (_radiusTiles > 0f && _ringAlpha > 0f)
        {
            var color = new Color(CyanGlow.R, CyanGlow.G, CyanGlow.B, (byte)(Math.Clamp(_ringAlpha, 0f, 1f) * 255));
            DrawRing(worldHandle, _centerWorld, _radiusTiles, MathF.Max(2f / 32f, _ringThicknessTiles * 0.6f), color);
        }
    }

    // Drawing helpers
    private static void DrawRing(DrawingHandleWorld handle, Vector2 center, float radius, float thickness, Color color)
    {
        const int steps = 8;
        for (int i = 0; i < steps; i++)
        {
            float f = i / (float)(steps - 1);
            float r = radius - f * thickness;
            byte a = (byte)(color.A * (1f - f));
            DrawCircleOutline(handle, center, MathF.Max(0f, r), new Color(color.R, color.G, color.B, a));
        }
    }

    private static void DrawCircleOutline(DrawingHandleWorld handle, Vector2 center, float radius, Color color, int segments = 128)
    {
        if (radius <= 0f)
            return;

        var prev = center + new Vector2(radius, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float theta = (float)(i * (2 * Math.PI / segments));
            var next = center + new Vector2(MathF.Cos(theta) * radius, MathF.Sin(theta) * radius);
            handle.DrawLine(prev, next, color);
            prev = next;
        }
    }
}
