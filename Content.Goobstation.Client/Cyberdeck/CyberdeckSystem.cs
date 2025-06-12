// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Shared.Cyberdeck;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Cyberdeck;

public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private CyberdeckOverlay _overlay = default!;
    private EntityQuery<CyberdeckOverlayComponent> _users;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new CyberdeckOverlay();
        _users = GetEntityQuery<CyberdeckOverlayComponent>();

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CyberdeckOverlayComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CyberdeckOverlayComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent args)
    {
        if (!_users.HasComp(args.Entity))
            return;

        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args) =>
        _overlayManager.RemoveOverlay(_overlay);

    private void OnStartup(Entity<CyberdeckOverlayComponent> ent, ref ComponentStartup args)
    {
        if (!_users.HasComp(_player.LocalEntity))
            return;

        _overlayManager.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<CyberdeckOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (!_users.HasComp(_player.LocalEntity))
            return;

        _overlayManager.RemoveOverlay(_overlay);
    }

    protected override void ShutdownProjection(Entity<CyberdeckProjectionComponent?>? ent) { }

    protected override void UpdateAlert(Entity<CyberdeckUserComponent> ent) { }
}
