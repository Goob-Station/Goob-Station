// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.Overlays;
using Content.Goobstation.Shared.SpellCard;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.SpellCard;


public sealed class SpellCardSystem : SharedSpellCardSystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly SpellCardOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        _overlayMan.AddOverlay(_overlay);

        SubscribeNetworkEvent<SpellCardAnimationEvent>(OnStartAnimation);

        SubscribeLocalEvent<SpellCardRecipientComponent, ComponentInit>(OnActorInit);
        SubscribeLocalEvent<SpellCardRecipientComponent, ComponentShutdown>(OnActorShutdown);

        SubscribeLocalEvent<SpellCardRecipientComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SpellCardRecipientComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnStartAnimation(SpellCardAnimationEvent ev)
    {
        _overlay.AnimationQueue.Enqueue(ev.AnimationData);
    }

    private void OnPlayerAttached(EntityUid uid, SpellCardRecipientComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SpellCardRecipientComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnActorInit(EntityUid uid, SpellCardRecipientComponent component, ComponentInit args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnActorShutdown(EntityUid uid, SpellCardRecipientComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
