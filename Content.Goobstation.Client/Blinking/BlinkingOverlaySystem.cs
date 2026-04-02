// SPDX-FileCopyrightText: 2026 Site-14 Contributors
//
// SPDX-License-Identifier: MPL-2.0
//
// Additional Use Restrictions apply:
// See /LICENSES/SITE14-ADDENDUM.md

using Content.Goobstation.Client.Blinking.Overlays;
using Content.Goobstation.Shared.Blinking;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Blinking;

public sealed class BlinkingOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private BlinkBlurOverlay _blurOverlay = default!;
    private BlinkAnimationOverlay _animationOverlay = default!;
    private BlinkIndicatorOverlay _indicatorOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkingComponent, ComponentInit>(OnBlinkingInit);
        SubscribeLocalEvent<BlinkingComponent, ComponentShutdown>(OnBlinkingShutdown);
        SubscribeLocalEvent<BlinkingComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<BlinkingComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _blurOverlay = new BlinkBlurOverlay();
        _animationOverlay = new BlinkAnimationOverlay();
        _indicatorOverlay = new BlinkIndicatorOverlay();
    }

    private void OnBlinkingInit(EntityUid uid, BlinkingComponent comp, ComponentInit args)
    {
        if (_playerManager.LocalEntity == uid)
            AddOverlays();
    }

    private void OnBlinkingShutdown(EntityUid uid, BlinkingComponent comp, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
            RemoveOverlays();
    }

    private void OnPlayerAttached(EntityUid uid, BlinkingComponent comp, LocalPlayerAttachedEvent args)
    {
        AddOverlays();
    }

    private void OnPlayerDetached(EntityUid uid, BlinkingComponent comp, LocalPlayerDetachedEvent args)
    {
        RemoveOverlays();
    }

    private void AddOverlays()
    {
        _overlayMan.AddOverlay(_blurOverlay);
        _overlayMan.AddOverlay(_animationOverlay);
        _overlayMan.AddOverlay(_indicatorOverlay);
    }

    private void RemoveOverlays()
    {
        _overlayMan.RemoveOverlay(_blurOverlay);
        _overlayMan.RemoveOverlay(_animationOverlay);
        _overlayMan.RemoveOverlay(_indicatorOverlay);
    }
}
