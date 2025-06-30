// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.ChronoLegionnaire.Overlays;
using Content.Goobstation.Shared.ChronoLegionnaire.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.ChronoLegionnaire;

public sealed class StasisOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private StasisOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InsideStasisComponent, ComponentInit>(OnStasisInit);
        SubscribeLocalEvent<InsideStasisComponent, ComponentShutdown>(OnStasisShutdown);

        SubscribeLocalEvent<InsideStasisComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<InsideStasisComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(Entity<InsideStasisComponent> stasised, ref LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<InsideStasisComponent> stasised, ref LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnStasisInit(Entity<InsideStasisComponent> stasised, ref ComponentInit args)
    {
        if (_player.LocalEntity == stasised)
            _overlayManager.AddOverlay(_overlay);
    }

    private void OnStasisShutdown(Entity<InsideStasisComponent> stasised, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == stasised)
        {
            _overlayManager.RemoveOverlay(_overlay);
        }
    }
}