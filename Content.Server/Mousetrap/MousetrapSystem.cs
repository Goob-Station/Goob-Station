// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 portfiend <109661617+portfiend@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Damage.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Abilities;
using Content.Shared.Interaction.Events;
using Content.Shared.Mousetrap;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Server.Mousetrap;

public sealed class MousetrapSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MousetrapComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<MousetrapComponent, BeforeDamageUserOnTriggerEvent>(BeforeDamageOnTrigger);
        SubscribeLocalEvent<MousetrapComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<MousetrapComponent, TriggerEvent>(OnTrigger);
    }

    private void OnUseInHand(EntityUid uid, MousetrapComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        component.IsActive = !component.IsActive;
        _popupSystem.PopupEntity(component.IsActive
            ? Loc.GetString("mousetrap-on-activate")
            : Loc.GetString("mousetrap-on-deactivate"),
            uid,
            args.User);

        UpdateVisuals(uid);

        args.Handled = true;
    }

    private void OnStepTriggerAttempt(EntityUid uid, MousetrapComponent component, ref StepTriggerAttemptEvent args)
    {
        // DeltaV: Entities with this component always trigger mouse traps, even if wearing shoes
        if (HasComp<AlwaysTriggerMousetrapComponent>(args.Tripper))
            args.Cancelled = false;

        args.Continue |= component.IsActive;
    }

    private void BeforeDamageOnTrigger(EntityUid uid, MousetrapComponent component, BeforeDamageUserOnTriggerEvent args)
    {
        if (TryComp(args.Tripper, out PhysicsComponent? physics) && physics.Mass != 0)
        {
            // The idea here is inverse,
            // Small - big damage,
            // Large - small damage
            // yes i punched numbers into a calculator until the graph looked right
            var scaledDamage = -50 * Math.Atan(physics.Mass - component.MassBalance) + (25 * Math.PI);
            args.Damage *= scaledDamage;
        }
    }

    private void OnTrigger(EntityUid uid, MousetrapComponent component, TriggerEvent args)
    {
        component.IsActive = false;
        UpdateVisuals(uid);
    }

    private void UpdateVisuals(EntityUid uid, MousetrapComponent? mousetrap = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref mousetrap, ref appearance, false))
        {
            return;
        }

        _appearance.SetData(uid, MousetrapVisuals.Visual,
            mousetrap.IsActive ? MousetrapVisuals.Armed : MousetrapVisuals.Unarmed, appearance);
    }
}