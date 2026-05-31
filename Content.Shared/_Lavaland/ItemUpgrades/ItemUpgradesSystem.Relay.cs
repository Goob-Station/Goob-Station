using Content.Goobstation.Common.Weapons;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared._Lavaland.ItemUpgrades.Components;
using Content.Shared._Lavaland.ItemUpgrades.Events;
using Content.Shared._Lavaland.Weapons;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared.Actions;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._Lavaland.ItemUpgrades;

public sealed partial class ItemUpgradesSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<ItemUpgradeableComponent, GunRefreshModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, GunShotEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, ProjectileShotEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, GetRelayMeleeWeaponEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, GetMeleeDamageEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, MeleeHitEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, GetLightAttackRangeEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, GetMeleeAttackRateEvent>(RelayEvent);
        SubscribeLocalEvent<ItemUpgradeableComponent, GetItemActionsEvent>(RelayGetActionEvent);
    }

    private void RelayEvent<T>(Entity<ItemUpgradeableComponent> ent, ref T args) where T : notnull
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            var beforeEv = new BeforeItemUpgradeRelayEvent();
            RaiseLocalEvent(upgrade, ref beforeEv);
            if (beforeEv.Cancelled)
                continue;

            RaiseLocalEvent(upgrade, ref args);
        }
    }

    // Because of how action container work we need that workaround for GetItemActionsEvent
    private void RelayGetActionEvent(Entity<ItemUpgradeableComponent> ent, ref GetItemActionsEvent args)
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
}
