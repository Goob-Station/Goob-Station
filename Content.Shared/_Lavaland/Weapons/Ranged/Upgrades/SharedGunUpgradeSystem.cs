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
using Content.Goobstation.Common.Weapons;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared.Actions;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades;

public abstract partial class SharedGunUpgradeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UpgradeableWeaponComponent, EntInsertedIntoContainerMessage>(OnUpgradeInserted);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttemptEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<UpgradeableWeaponComponent, GunRefreshModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GunShotEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ProjectileShotEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetRelayMeleeWeaponEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetMeleeDamageEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, MeleeHitEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetLightAttackRangeEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetMeleeAttackRateEvent>(RelayEvent);

        SubscribeLocalEvent<UpgradeableWeaponComponent, GetItemActionsEvent>(RelayGetActionEvent);

        SubscribeLocalEvent<GunUpgradeComponent, ExaminedEvent>(OnUpgradeExamine);

        InitializeUpgrades();
    }

    private void RelayEvent<T>(Entity<UpgradeableWeaponComponent> ent, ref T args) where T : notnull
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            RaiseLocalEvent(upgrade, ref args);
        }
    }

    // Because of how action container work we need that workaround for GetItemActionsEvent
    private void RelayGetActionEvent(Entity<UpgradeableWeaponComponent> ent, ref GetItemActionsEvent args)
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            var ev = new GetItemActionsEvent(_actionContainer, args.User, upgrade.Owner, isEquipping: args.IsEquipping);
            RaiseLocalEvent(upgrade.Owner, ev);

            if (ev.Actions.Count == 0)
                continue;

            if (!args.IsEquipping)
            {
                _actions.RemoveProvidedActions(args.User, upgrade.Owner);
                _actions.SaveActions(args.User);
                continue;
            }

            _actions.GrantActions(args.User, ev.Actions, upgrade.Owner);
            _actions.LoadActions(args.User);
        }
    }

    private void OnExamine(Entity<UpgradeableWeaponComponent> ent, ref ExaminedEvent args)
    {
        var usedCapacity = 0;
        using (args.PushGroup(nameof(UpgradeableWeaponComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                if (upgrade.Comp.InsertedTextType != null)
                    args.PushMarkup(Loc.GetString(upgrade.Comp.InsertedTextType.Value, ("name", Loc.GetString(upgrade.Comp.Name))));
                if (upgrade.Comp.CapacityCost != null)
                    usedCapacity += upgrade.Comp.CapacityCost.Value;
            }

            if (ent.Comp.MaxUpgradeCapacity != null)
                args.PushMarkup(Loc.GetString("upgradeable-gun-total-remaining-capacity", ("value", ent.Comp.MaxUpgradeCapacity.Value - usedCapacity)));
        }
    }

    private void OnUpgradeExamine(Entity<GunUpgradeComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineTextType != null) // TODO add a list of all weapon types that this gun upgrade can be inserted to
            args.PushMarkup(Loc.GetString(ent.Comp.ExamineTextType.Value, ("name", Loc.GetString(ent.Comp.Name))));

        if (ent.Comp.CapacityCost != null)
            args.PushMarkup(Loc.GetString("gun-upgrade-capacity-cost", ("value", ent.Comp.CapacityCost.Value)));
    }

    private void OnUpgradeInserted(Entity<UpgradeableWeaponComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // Update some characteristics here.
        if (TryComp(ent.Owner, out GunComponent? gun))
            _gun.RefreshModifiers((ent.Owner, gun));
    }

    private void OnItemSlotInsertAttemptEvent(Entity<UpgradeableWeaponComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
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

    public HashSet<Entity<GunUpgradeComponent>> GetCurrentUpgrades(Entity<UpgradeableWeaponComponent> ent, ItemSlotsComponent? itemSlots = null)
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
