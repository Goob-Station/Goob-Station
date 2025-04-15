// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Systems;

public sealed class MovementIgnoreGravitySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MovementIgnoreGravityComponent, ComponentGetState>(GetState);
        SubscribeLocalEvent<MovementIgnoreGravityComponent, ComponentHandleState>(HandleState);
        SubscribeLocalEvent<MovementAlwaysTouchingComponent, CanWeightlessMoveEvent>(OnWeightless);
    }

    private void OnWeightless(EntityUid uid, MovementAlwaysTouchingComponent component, ref CanWeightlessMoveEvent args)
    {
        args.CanMove = true;
    }

    private void HandleState(EntityUid uid, MovementIgnoreGravityComponent component, ref ComponentHandleState args)
    {
        if (args.Next is null)
            return;

        component.Weightless = ((MovementIgnoreGravityComponentState) args.Next).Weightless;
    }

    private void GetState(EntityUid uid, MovementIgnoreGravityComponent component, ref ComponentGetState args)
    {
        args.State = new MovementIgnoreGravityComponentState(component);
    }
}