// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Content.Client.MalfAI.Overlays;
using Content.Shared.MalfAI;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Shared.Audio;
using Robust.Shared.Maths;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Vector2 = System.Numerics.Vector2;

namespace Content.Client.MalfAI;

/// <summary>
/// Listens for the doomsday ripple start event and attaches an overlay
/// that renders a screen-space distortion ripple with a cyan glow ring,
/// synced to server time.
/// </summary>
public sealed class MalfAiDoomsdayRippleClientSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    // Current effect state
    private bool _active;
    private MapId _mapId;
    private Vector2 _originWorld;
    private double _serverStartSeconds;
    private TimeSpan _duration;
    private float _maxRadiusTiles;
    private bool _centerFlash;

    // Overlay instance
    private MalfAiDoomsdayRippleOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<MalfAiDoomsdayRippleStartedEvent>(OnRippleStarted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_active || _overlay == null)
            return;

        // Stop and remove after duration elapses
        var now = _timing.CurTime.TotalSeconds;
        var elapsed = Math.Max(0, now - _serverStartSeconds);
        if (elapsed >= _duration.TotalSeconds)
        {
            EndOverlay();
        }
    }

    private void OnRippleStarted(MalfAiDoomsdayRippleStartedEvent ev)
    {
        _active = true;
        _mapId = ev.MapId;
        _originWorld = ev.OriginWorld;
        _serverStartSeconds = ev.ServerStartSeconds;
        _duration = ev.Duration;
        _maxRadiusTiles = ev.MaxRadiusTiles;
        _centerFlash = ev.CenterFlash;

        if (_overlay == null)
        {
            _overlay = new MalfAiDoomsdayRippleOverlay(this);
            _overlays.AddOverlay(_overlay);
        }

        // Placeholder SFX on start. This uses a generic effect; replace with a dedicated asset as desired.
        // If missing, the audio system will simply not play anything.
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg"), Filter.Local(), false);
    }

    private void EndOverlay()
    {
        _active = false;
        if (_overlay != null)
        {
            _overlays.RemoveOverlay(_overlay);
            _overlay = null;
        }
    }

    // State accessors for the overlay
    public bool IsActive => _active;
    public double ServerStartSeconds => _serverStartSeconds;
    public TimeSpan Duration => _duration;
    public float MaxRadiusTiles => _maxRadiusTiles;
    public Vector2 OriginWorld => _originWorld;
    public bool CenterFlash => _centerFlash;
}
