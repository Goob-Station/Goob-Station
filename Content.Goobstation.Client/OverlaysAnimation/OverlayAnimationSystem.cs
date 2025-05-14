// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.Overlays;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.OverlaysAnimation.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.OverlaysAnimation;


public sealed partial class OverlayAnimationSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private SpecialAnimationOverlay _overlay = default!;

    public bool AnimationsEnabled = true;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OverlaysAnimationViewerComponent, ComponentInit>(OnActorInit);
        SubscribeLocalEvent<OverlaysAnimationViewerComponent, ComponentShutdown>(OnActorShutdown);
        SubscribeLocalEvent<OverlaysAnimationViewerComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<OverlaysAnimationViewerComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<OverlaySpriteComponent, ComponentStartup>(OnSpriteAnimationStartup);

        _overlay = new();

        Subs.CVar(_cfg, GoobCVars.OverlayAnimationsEnabled, value => AnimationsEnabled = value, true);
    }

    private void OnPlayerAttached(EntityUid uid, OverlaysAnimationViewerComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, OverlaysAnimationViewerComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnActorInit(EntityUid uid, OverlaysAnimationViewerComponent component, ComponentInit args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnActorShutdown(EntityUid uid, OverlaysAnimationViewerComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnSpriteAnimationStartup(EntityUid uid, OverlaySpriteComponent component, ComponentStartup args)
    {
        // Copy the sprite from the override entity, if it was specified by other system before startup.
        if (component.OverrideSprite == null)
            return;

        var source = GetEntity(component.OverrideSprite.Value);

        if (!TryComp<SpriteComponent>(source, out var sourceSprite))
            return;

        var animSprite = EnsureComp<SpriteComponent>(uid);
        animSprite.CopyFrom(sourceSprite);
    }
}
