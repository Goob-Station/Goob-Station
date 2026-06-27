// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.Overlays;
using Content.Goobstation.Shared.SpecialAnimation;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.SpecialAnimation;


public sealed class SpecialAnimationSystem : SharedSpecialAnimationSystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private SpecialAnimationOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new SpecialAnimationOverlay(_sprite);
        _overlayMan.AddOverlay(_overlay);

        SubscribeNetworkEvent<SpecialAnimationEvent>(OnStartAnimation);

        SubscribeLocalEvent<SpecialAnimationViewerComponent, ComponentInit>(OnActorInit);
        SubscribeLocalEvent<SpecialAnimationViewerComponent, ComponentShutdown>(OnActorShutdown);

        SubscribeLocalEvent<SpecialAnimationViewerComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SpecialAnimationViewerComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnStartAnimation(SpecialAnimationEvent ev)
    {
        lock (_overlay.AnimationQueue)
        {
            _overlay.AnimationQueue.Enqueue(ev.AnimationData);
        }
    }

    private void OnPlayerAttached(EntityUid uid, SpecialAnimationViewerComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SpecialAnimationViewerComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnActorInit(EntityUid uid, SpecialAnimationViewerComponent component, ComponentInit args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnActorShutdown(EntityUid uid, SpecialAnimationViewerComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
