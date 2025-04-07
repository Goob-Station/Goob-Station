// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory.Events;
using Content.Shared.Ninja.Components;

namespace Content.Shared.Ninja.Systems;

/// <summary>
/// System for katana binding and dash events. Recalling is handled by the suit.
/// </summary>
public sealed class EnergyKatanaSystem : EntitySystem
{
    [Dependency] private readonly SharedSpaceNinjaSystem _ninja = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnergyKatanaComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<EnergyKatanaComponent, CheckDashEvent>(OnCheckDash);
    }

    /// <summary>
    /// When equipped by a ninja, try to bind it.
    /// </summary>
    private void OnEquipped(Entity<EnergyKatanaComponent> ent, ref GotEquippedEvent args)
    {
        _ninja.BindKatana(args.Equipee, ent);
    }

    private void OnCheckDash(Entity<EnergyKatanaComponent> ent, ref CheckDashEvent args)
    {
        if (!_ninja.IsNinja(args.User))
            args.Cancelled = true;
    }
}