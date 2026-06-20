// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
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
