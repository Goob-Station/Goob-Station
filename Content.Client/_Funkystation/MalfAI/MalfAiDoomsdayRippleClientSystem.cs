// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Client._Funkystation.MalfAI.Overlays;
using Content.Shared._Funkystation.MalfAI;
using Robust.Client.Graphics;

namespace Content.Client._Funkystation.MalfAI;

/// <summary>
/// Client system that receives doomsday ripple events and displays them as a visual overlay.
/// </summary>
public sealed class MalfAiDoomsdayRippleClientSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private MalfAiDoomsdayRippleOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<MalfAiDoomsdayRippleStartedEvent>(OnRippleStarted);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_overlay != null)
        {
            _overlayManager.RemoveOverlay(_overlay);
            _overlay = null;
        }
    }

    private void OnRippleStarted(MalfAiDoomsdayRippleStartedEvent ev)
    {
        if (_overlay == null)
        {
            _overlay = new MalfAiDoomsdayRippleOverlay();
            _overlayManager.AddOverlay(_overlay);
        }

        _overlay.AddRipple(ev.OriginWorld, ev.ServerStartSeconds, ev.Duration, ev.MaxRadiusTiles);
    }
}
