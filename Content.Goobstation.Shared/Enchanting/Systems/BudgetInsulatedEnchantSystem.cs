// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Electrocution;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Controls <see cref="BudgetInsulatedEnchantComponent"/> values with the enchant level.
/// </summary>
public sealed class BudgetInsulatedEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BudgetInsulatedEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<BudgetInsulatedEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
        SubscribeLocalEvent<BudgetInsulatedEnchantComponent, ElectrocutionAttemptEvent>(OnElectrocutionAttempt);
    }

    private void OnAdded(Entity<BudgetInsulatedEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        Modify(ent, args.Level);
    }

    private void OnUpgraded(Entity<BudgetInsulatedEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        Modify(ent, args.Level);
    }

    private void OnElectrocutionAttempt(Entity<BudgetInsulatedEnchantComponent> ent, ref ElectrocutionAttemptEvent args)
    {
        args.SiemensCoefficient = ent.Comp.NextCoefficient;

        // should be enough time for server to network it before next shock, otherwise mispredict ops in the future
        // (currently doesn't matter as electrocution is not predicted)
        Cycle(ent);
    }

    private void Modify(Entity<BudgetInsulatedEnchantComponent> ent, int level)
    {
        // Insulated? IV becomes 40% real 60% nostuns
        var max = 5f - (float) level;
        ent.Comp.Coefficients.RemoveAll(n => n > max);
        Cycle(ent);
    }

    private void Cycle(Entity<BudgetInsulatedEnchantComponent> ent)
    {
        ent.Comp.NextCoefficient = _random.Pick(ent.Comp.Coefficients);
        Dirty(ent);
    }
}
