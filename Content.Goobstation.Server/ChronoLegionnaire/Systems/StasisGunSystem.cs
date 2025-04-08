// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.ChronoLegionnaire.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Throwing;

namespace Content.Goobstation.Server.ChronoLegionnaire.Systems;

public sealed partial class StasisGunSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StasisGunComponent, DroppedEvent>(OnWeaponDrop);
        SubscribeLocalEvent<StasisGunComponent, ThrownEvent>(OnWeaponThrown);
    }

    /// <summary>
    /// Return weapon on belt when it dropped
    /// </summary>
    private void OnWeaponDrop(Entity<StasisGunComponent> gun, ref DroppedEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<Shared.ChronoLegionnaire.Components.StasisImmunityComponent>(args.User))
            return;

        args.Handled = _inventory.TryEquip(args.User, gun, gun.Comp.ReturningSlot, predicted: true, force: true);
    }

    /// <summary>
    /// Return weapon on belt when it thrown
    /// </summary>
    private void OnWeaponThrown(Entity<StasisGunComponent> gun, ref ThrownEvent args)
    {
        if (!HasComp<Shared.ChronoLegionnaire.Components.StasisImmunityComponent>(args.User))
            return;

        _inventory.TryEquip(args.User.Value, gun, gun.Comp.ReturningSlot, predicted: true, force: true);
    }


}