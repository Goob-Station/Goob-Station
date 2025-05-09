// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Shared.Enchanting.Components;

namespace Content.Goidastation.Shared.Enchanting.Systems;

/// <summary>
/// Adds or removes components from an item enchanted with <see cref="ComponentsEnchantComponent"/>
/// Multiple levels do nothing.
/// </summary>
public sealed class ComponentsEnchantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComponentsEnchantComponent, EnchantAddedEvent>(OnAdded);
    }

    private void OnAdded(Entity<ComponentsEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        if (ent.Comp.Added is {} added)
            EntityManager.AddComponents(args.Item, added);
        if (ent.Comp.Removed is {} removed)
            EntityManager.RemoveComponents(args.Item, removed);
    }
}
