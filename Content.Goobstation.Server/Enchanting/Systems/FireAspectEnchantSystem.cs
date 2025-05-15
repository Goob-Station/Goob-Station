// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Server.Atmos.Components;

namespace Content.Goobstation.Server.Enchanting.Systems;

/// <summary>
/// Controls <see cref="IgniteOnMeleeHitComponent"/> fire stacks with enchant level.
/// </summary>
public sealed class FireAspectEnchantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireAspectEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<FireAspectEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
    }

    private void OnAdded(Entity<FireAspectEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        Add(ent, ent.Comp.StacksPerLevel * args.Level);
    }

    private void OnUpgraded(Entity<FireAspectEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        Add(ent, ent.Comp.StacksPerLevel * (args.Level - args.OldLevel));
    }

    private void Add(EntityUid uid, float stacks)
    {
        EnsureComp<IgniteOnMeleeHitComponent>(uid).FireStacks += stacks;
    }
}
