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

using Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using System.Linq;
using Content.Shared._Goobstation.Weapons.Ranged;
using Robust.Shared.Containers;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades;

public abstract partial class SharedGunUpgradeSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UpgradeableGunComponent, EntInsertedIntoContainerMessage>(OnUpgradeInserted);
        SubscribeLocalEvent<UpgradeableGunComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttemptEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<UpgradeableGunComponent, GunRefreshModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, GunShotEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, ProjectileShotEvent>(RelayEvent);

        SubscribeLocalEvent<GunUpgradeComponent, ExaminedEvent>(OnUpgradeExamine);

        InitializeUpgrades();
    }

    private void RelayEvent<T>(Entity<UpgradeableGunComponent> ent, ref T args) where T : notnull
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            RaiseLocalEvent(upgrade, ref args);
        }
    }

    private void OnExamine(Entity<UpgradeableGunComponent> ent, ref ExaminedEvent args)
    {
        var usedCapacity = 0;
        using (args.PushGroup(nameof(UpgradeableGunComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                args.PushMarkup(Loc.GetString(upgrade.Comp.ExamineText));
                if (upgrade.Comp.CapacityCost != null)
                    usedCapacity += upgrade.Comp.CapacityCost.Value;
            }

            if (ent.Comp.MaxUpgradeCapacity != null)
                args.PushMarkup(Loc.GetString("upgradeable-gun-total-remaining-capacity", ("value", ent.Comp.MaxUpgradeCapacity.Value - usedCapacity)));
        }
    }

    private void OnUpgradeExamine(Entity<GunUpgradeComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineText));
        if (ent.Comp.CapacityCost != null)
            args.PushMarkup(Loc.GetString("gun-upgrade-examine-text-capacity-cost", ("value", ent.Comp.CapacityCost.Value)));
    }

    private void OnUpgradeInserted(Entity<UpgradeableGunComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // Update some characteristics here.
        _gun.RefreshModifiers(ent.Owner);
    }

    private void OnItemSlotInsertAttemptEvent(Entity<UpgradeableGunComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        // TODO: Figure out how to kill the interaction verbs bypassing checks, yet also allowing
        // for non-duplicate popups to the user when they interact without having to do all of this crap twice.
        if (!TryComp<GunUpgradeComponent>(args.Item, out var upgradeComp)
            || !TryComp<ItemSlotsComponent>(ent, out var itemSlots))
            return;

        var currentUpgrades = GetCurrentUpgrades(ent, itemSlots);
        var totalCapacityCost = currentUpgrades.Sum(upgrade => upgrade.Comp.CapacityCost);
        if (totalCapacityCost + upgradeComp.CapacityCost > ent.Comp.MaxUpgradeCapacity)
        {
            args.Cancelled = true;
            return;
        }

        var itemProto = MetaData(args.Item).EntityPrototype?.ID;
        foreach (var itemSlot in itemSlots.Slots.Values)
        {
            if (itemSlot is not { HasItem: true, Item: { } existingItem }
                || MetaData(existingItem).EntityPrototype?.ID != itemProto
                || !upgradeComp.Unique)
                continue;

            args.Cancelled = true;
            break;
        }
    }

    public HashSet<Entity<GunUpgradeComponent>> GetCurrentUpgrades(Entity<UpgradeableGunComponent> ent, ItemSlotsComponent? itemSlots = null)
    {
        if (!Resolve(ent, ref itemSlots))
            return [];

        var upgrades = new HashSet<Entity<GunUpgradeComponent>>();

        foreach (var itemSlot in itemSlots.Slots.Values)
        {
            if (itemSlot is { HasItem: true, Item: { } item }
                && TryComp<GunUpgradeComponent>(item, out var upgradeComp))
                upgrades.Add((item, upgradeComp));
        }

        return upgrades;
    }
}
