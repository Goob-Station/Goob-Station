// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Shadowling;

public sealed class EnthrallSystem : SharedEnthrallSystem
{
    private EnthrallOverlay _overlay = default!;

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<ThrallComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ThrallComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, ThrallComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, ThrallComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnInit(EntityUid uid, ThrallComponent component, ComponentInit init)
    {
        if (_playerManager.LocalEntity != uid
            || EntityManager.HasComponent<LesserShadowlingComponent>(uid))
            return;

        _overlay.ReceiveEnthrall(5f);
        _overlayManager.AddOverlay(_overlay);
    }
}
