// SPDX-FileCopyrightText: 2024 Angelo Fallaria <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Movement;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Goobstation.Shared.Movement.Systems;

public sealed class InnateSpeedModifierSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InnateSpeedModifierComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<InnateSpeedModifierComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    private void OnRefreshMovementSpeed(EntityUid uid, InnateSpeedModifierComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.WalkModifier, component.SprintModifier, bypassImmunity: true);
    }

    private void OnStartup(EntityUid uid, InnateSpeedModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<MovementSpeedModifierComponent>(uid, out var move))
            return;

        _movement.RefreshMovementSpeedModifiers(uid, move);
    }
}
