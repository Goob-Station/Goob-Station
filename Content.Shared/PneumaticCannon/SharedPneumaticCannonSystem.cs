// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Shared.PneumaticCannon;

public abstract class SharedPneumaticCannonSystem : EntitySystem
{
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PneumaticCannonComponent, AttemptShootEvent>(OnAttemptShoot);
    }

    private void OnAttemptShoot(EntityUid uid, PneumaticCannonComponent component, ref AttemptShootEvent args)
    {
        // if the cannon doesn't need gas then it will always predict firing
        if (component.GasUsage == 0f)
            return;

        // pneumatic cannon usually doesn't shoot bullets
        args.ThrowItems = component.ThrowItems;

        // we don't have atmos on shared, so just predict by the existence of a slot item
        // server will handle auto ejecting/not adding the slot item if it doesnt have enough gas,
        // so this won't mispredict
        if (!Container.TryGetContainer(uid, PneumaticCannonComponent.TankSlotId, out var container) ||
            container is not ContainerSlot slot || slot.ContainedEntity is null)
        {
            args.Cancelled = true;
        }
    }
}