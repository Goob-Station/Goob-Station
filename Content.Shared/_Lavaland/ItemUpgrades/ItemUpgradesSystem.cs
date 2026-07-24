// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared._Lavaland.ItemUpgrades.Components;
using Content.Shared.Actions;
using Content.Shared.Weapons.Ranged.Components;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Shared._Lavaland.ItemUpgrades;

public sealed partial class ItemUpgradesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ItemUpgradeableComponent, EntInsertedIntoContainerMessage>(OnUpgradeInserted);
        SubscribeLocalEvent<ItemUpgradeableComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttemptEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<ItemUpgradeComponent, ExaminedEvent>(OnUpgradeExamine);

        InitializeRelay();
    }

    private void OnExamine(Entity<ItemUpgradeableComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(ItemUpgradeableComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                if (upgrade.Comp.InsertedTextType != null)
                    args.PushMarkup(Loc.GetString(upgrade.Comp.InsertedTextType.Value, ("name", Loc.GetString(upgrade.Comp.Name))));
            }
        }
    }

    private void OnUpgradeExamine(Entity<ItemUpgradeComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineTextType != null) // TODO add a list of all weapon types that this gun upgrade can be inserted to
            args.PushMarkup(Loc.GetString(ent.Comp.ExamineTextType.Value, ("name", Loc.GetString(ent.Comp.Name))));
    }

    private void OnUpgradeInserted(Entity<ItemUpgradeableComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // Update some characteristics here.
        if (TryComp(ent.Owner, out GunComponent? gun))
            _gun.RefreshModifiers((ent.Owner, gun));
    }

    private void OnItemSlotInsertAttemptEvent(Entity<ItemUpgradeableComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (!TryComp<ItemUpgradeComponent>(args.Item, out var upgradeComp)
            || !TryComp<ItemSlotsComponent>(ent, out var itemSlots))
            return;

        var currentUpgrades = GetCurrentUpgrades(ent, itemSlots);
        foreach (var curUpgrade in currentUpgrades)
        {
            if (upgradeComp.UniqueGroup == null
                || curUpgrade.Comp.UniqueGroup == null
                || upgradeComp.UniqueGroup != curUpgrade.Comp.UniqueGroup)
                continue;

            args.Cancelled = true;
            return;
        }
    }

    [PublicAPI]
    public HashSet<Entity<ItemUpgradeComponent>> GetCurrentUpgrades(Entity<ItemUpgradeableComponent> ent, ItemSlotsComponent? itemSlots = null)
    {
        if (!Resolve(ent, ref itemSlots))
            return [];

        var upgrades = new HashSet<Entity<ItemUpgradeComponent>>();

        foreach (var itemSlot in itemSlots.Slots.Values)
        {
            if (itemSlot is { HasItem: true, Item: { } item }
                && TryComp<ItemUpgradeComponent>(item, out var upgradeComp))
                upgrades.Add((item, upgradeComp));
        }

        return upgrades;
    }
}
