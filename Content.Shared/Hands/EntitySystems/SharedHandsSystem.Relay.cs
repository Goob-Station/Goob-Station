// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Camera;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;

// Goobstation using
using Content.Shared._Shitmed.Surgery;
using Content.Shared.Cuffs;
using Content.Shared.Heretic;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<HandsComponent, GetEyeOffsetRelayedEvent>(RelayEvent);
        SubscribeLocalEvent<HandsComponent, GetEyePvsScaleRelayedEvent>(RelayEvent);
        SubscribeLocalEvent<HandsComponent, RefreshMovementSpeedModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<HandsComponent, CheckMagicItemEvent>(RelayEvent); // goob edit - heretics
        SubscribeLocalEvent<HandsComponent, SurgerySanitizationEvent>(RelayEvent); // goob edit - heretics
        SubscribeLocalEvent<HandsComponent, SurgeryPainEvent>(RelayEvent); // goob edit - heretics
        SubscribeLocalEvent<HandsComponent, SurgeryIgnorePreviousStepsEvent>(RelayEvent); // goob edit - heretics

        // By-ref events.
        SubscribeLocalEvent<HandsComponent, ExtinguishEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, ProjectileReflectAttemptEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, HitScanReflectAttemptEvent>(RefRelayEvent);
        //SubscribeLocalEvent<HandsComponent, WieldAttemptEvent>(RefRelayEvent);
        //SubscribeLocalEvent<HandsComponent, UnwieldAttemptEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, TargetHandcuffedEvent>(RefRelayEvent);

        SubscribeLocalEvent<HandsComponent, RefreshEquipmentHudEvent<ShowHealthBarsComponent>>(RefRelayEvent); // goob edit - heretics
        SubscribeLocalEvent<HandsComponent, RefreshEquipmentHudEvent<ShowHealthIconsComponent>>(RefRelayEvent); // goob edit - heretics
    }

    private void RelayEvent<T>(Entity<HandsComponent> entity, ref T args) where T : EntityEventArgs
    {
        CoreRelayEvent(entity, ref args);
    }

    private void RefRelayEvent<T>(Entity<HandsComponent> entity, ref T args)
    {
        var ev = CoreRelayEvent(entity, ref args);
        args = ev.Args;
    }

    private HeldRelayedEvent<T> CoreRelayEvent<T>(Entity<HandsComponent> entity, ref T args)
    {
        var ev = new HeldRelayedEvent<T>(args);

        foreach (var held in EnumerateHeld(entity.AsNullable()))
        {
            RaiseLocalEvent(held, ref ev);
        }

        return ev;
    }
}
