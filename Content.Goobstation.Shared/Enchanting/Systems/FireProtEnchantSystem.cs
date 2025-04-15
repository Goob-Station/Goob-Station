// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Atmos;
using Content.Shared.Mobs.Components;
using Content.Shared.Temperature;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Handles fire + temperature events for <see cref="FireProtEnchantComponent"/>.
/// </summary>
public sealed class FireProtEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireProtEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<FireProtEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
        SubscribeLocalEvent<FireProtEnchantComponent, GetFireProtectionEvent>(OnGetFireProtection);
        SubscribeLocalEvent<FireProtEnchantComponent, ModifyChangedTemperatureEvent>(OnTemperatureChangeAttempt);
    }

    private void OnAdded(Entity<FireProtEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        Modify(ent, (float) args.Level);
    }

    private void OnUpgraded(Entity<FireProtEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        Modify(ent, (float) args.Level / (float) args.OldLevel);
    }

    private void Modify(Entity<FireProtEnchantComponent> ent, float factor)
    {
        ent.Comp.Reduction *= factor;
        ent.Comp.TempModifier = (float) Math.Pow(ent.Comp.TempModifier, factor);
        Dirty(ent);
    }

    private void OnGetFireProtection(Entity<FireProtEnchantComponent> ent, ref GetFireProtectionEvent args)
    {
        if (Ignored(ent, args.Target))
            return;

        args.Reduce(ent.Comp.Reduction);
    }

    private void OnTemperatureChangeAttempt(Entity<FireProtEnchantComponent> ent, ref ModifyChangedTemperatureEvent args)
    {
        // don't care about cooling
        if (args.TemperatureDelta < 0 || Ignored(ent, args.Target))
            return;

        args.TemperatureDelta *= ent.Comp.TempModifier;
    }

    private bool Ignored(EntityUid uid, EntityUid target)
    {
        // Fire Protection mouse will only protect the mouse not you
        var item = _enchanting.GetEnchantedItem(uid);
        return item != target && HasComp<MobStateComponent>(item);
    }
}
