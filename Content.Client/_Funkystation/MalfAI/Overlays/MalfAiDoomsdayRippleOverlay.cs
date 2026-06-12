// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

namespace Content.Client._Funkystation.MalfAI.Overlays;

/// <summary>
/// Client-side visual overlay that draws an expanding cyan ring as the doomsday ripple spreads.
/// </summary>
public sealed class MalfAiDoomsdayRippleOverlay : Overlay
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly List<RippleData> _activeRipples = new();

    public MalfAiDoomsdayRippleOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    public void AddRipple(Vector2 worldOrigin, double serverStartSeconds, float durationSeconds, float maxRadiusTiles)
    {
        _activeRipples.Add(new RippleData(worldOrigin, serverStartSeconds, durationSeconds, maxRadiusTiles));
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.MapId != _eyeManager.CurrentMap)
            return;

        var handle = args.WorldHandle;
        var now = _timing.CurTime.TotalSeconds;

        for (var i = _activeRipples.Count - 1; i >= 0; i--)
        {
            var ripple = _activeRipples[i];
            var elapsed = (float)(now - ripple.ServerStartSeconds);

            if (elapsed >= ripple.DurationSeconds)
            {
                _activeRipples.RemoveAt(i);
                continue;
            }

            var t = elapsed / ripple.DurationSeconds;
            var radius = t * ripple.MaxRadiusTiles;
            var alpha = 1f - t;

            var color = Color.Cyan.WithAlpha(alpha * 0.8f);
            handle.DrawCircle(ripple.WorldOrigin, radius, color, filled: false);

            // Inner ring for effect
            var innerRadius = Math.Max(0f, radius - 0.5f);
            handle.DrawCircle(ripple.WorldOrigin, innerRadius, Color.White.WithAlpha(alpha * 0.3f), filled: false);
        }
    }

    private sealed record RippleData(
        Vector2 WorldOrigin,
        double ServerStartSeconds,
        float DurationSeconds,
        float MaxRadiusTiles);
}
