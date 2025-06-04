// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Item;
using Content.Shared.Movement.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Item;

public sealed class GoobHeldSpeedModifierSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeldSpeedModifierComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HeldSpeedModifierComponent, ComponentRemove>(OnRemoved);
    }

    private void OnInit(Entity<HeldSpeedModifierComponent> ent, ref ComponentInit args)
    {
        if (_container.TryGetContainingContainer((ent, null, null), out var container))
            _movement.RefreshMovementSpeedModifiers(container.Owner);
    }

    private void OnRemoved(Entity<HeldSpeedModifierComponent> ent, ref ComponentRemove args)
    {
        if (_container.TryGetContainingContainer((ent, null, null), out var container))
            _movement.RefreshMovementSpeedModifiers(container.Owner);
    }
}
