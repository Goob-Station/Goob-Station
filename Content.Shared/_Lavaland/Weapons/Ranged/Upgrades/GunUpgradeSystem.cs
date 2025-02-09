using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades;

public sealed class GunUpgradeSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UpgradeableGunComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<UpgradeableGunComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttemptEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<UpgradeableGunComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UpgradeableGunComponent, GunRefreshModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, GunShotEvent>(RelayEvent);

        SubscribeLocalEvent<GunUpgradeFireRateComponent, GunRefreshModifiersEvent>(OnFireRateRefresh);
        SubscribeLocalEvent<GunComponentUpgrateComponent, GunRefreshModifiersEvent>(OnCompsRefresh);
        SubscribeLocalEvent<GunUpgradeSpeedComponent, GunRefreshModifiersEvent>(OnSpeedRefresh);
        SubscribeLocalEvent<GunUpgradeDamageComponent, GunShotEvent>(OnDamageGunShot);
        SubscribeLocalEvent<GunUpgradeComponentsComponent, GunShotEvent>(OnDamageGunShotComps);
        SubscribeLocalEvent<GunUpgradeVampirismComponent, GunShotEvent>(OnVampirismGunShot);
        SubscribeLocalEvent<ProjectileVampirismComponent, ProjectileHitEvent>(OnVampirismProjectileHit);
    }

    private void OnMapInit(EntityUid uid, UpgradeableGunComponent component, MapInitEvent args)
    {
        // Create item slots for each possible upgrade slot
        for (var i = 0; i < component.MaxUpgradeCount; i++)
        {
            var slotId = $"{component.UpgradesContainerId}-{i}";
            var slot = new ItemSlot
            {
                Whitelist = component.Whitelist,
                Swap = false,
                EjectOnBreak = true,
                Name = Loc.GetString("upgradeable-gun-slot-name", ("value", i + 1))
            };

            _itemSlots.AddItemSlot(uid, slotId, slot);
        }
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
        using (args.PushGroup(nameof(UpgradeableGunComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                args.PushMarkup(Loc.GetString(upgrade.Comp.ExamineText));
            }
        }
    }

    private void OnInteractUsing(Entity<UpgradeableGunComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled
            || !HasComp<GunUpgradeComponent>(args.Used)
            || !TryComp<ItemSlotsComponent>(ent, out var itemSlots)
            || _entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Used))
            return;

        if (GetCurrentUpgrades(ent).Count >= ent.Comp.MaxUpgradeCount)
        {
            _popup.PopupPredicted(Loc.GetString("upgradeable-gun-popup-upgrade-limit"), ent, args.User);
            return;
        }

        if (_itemSlots.TryInsertEmpty((ent.Owner, itemSlots), args.Used, args.User, true))
            _gun.RefreshModifiers(ent.Owner);

        args.Handled = true;
    }

    private void OnItemSlotInsertAttemptEvent(Entity<UpgradeableGunComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        var itemProto = MetaData(args.Item).EntityPrototype?.ID;
        for (var i = 0; i < ent.Comp.MaxUpgradeCount; i++)
        {
            var slotId = $"{ent.Comp.UpgradesContainerId}-{i}";

            if (_itemSlots.GetItemOrNull(ent, slotId) is { } existingItem
                && MetaData(existingItem).EntityPrototype?.ID == itemProto)
            {
                args.Cancelled = true;
                break;
            }
        }
    }

    private void OnFireRateRefresh(Entity<GunUpgradeFireRateComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.FireRate *= ent.Comp.Coefficient;
    }

    private void OnCompsRefresh(Entity<GunComponentUpgrateComponent> ent, ref GunRefreshModifiersEvent args)
    {
        EntityManager.AddComponents(args.Gun, ent.Comp.Components);
    }

    private void OnSpeedRefresh(Entity<GunUpgradeSpeedComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.ProjectileSpeed *= ent.Comp.Coefficient;
    }

    private void OnDamageGunShot(Entity<GunUpgradeDamageComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (TryComp<ProjectileComponent>(ammo, out var proj))
                proj.Damage += ent.Comp.Damage;
        }
    }
    private void OnDamageGunShotComps(Entity<GunUpgradeComponentsComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (HasComp<ProjectileComponent>(ammo))
                EntityManager.AddComponents(ammo.Value, ent.Comp.Components);
        }
    }

    private void OnVampirismGunShot(Entity<GunUpgradeVampirismComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (!HasComp<ProjectileComponent>(ammo))
                continue;

            var comp = EnsureComp<ProjectileVampirismComponent>(ammo.Value);
            comp.DamageOnHit = ent.Comp.DamageOnHit;
        }
    }

    private void OnVampirismProjectileHit(Entity<ProjectileVampirismComponent> ent, ref ProjectileHitEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Target))
            return;
        _damage.TryChangeDamage(args.Shooter, ent.Comp.DamageOnHit);
    }

    public HashSet<Entity<GunUpgradeComponent>> GetCurrentUpgrades(Entity<UpgradeableGunComponent> ent)
    {
        var upgrades = new HashSet<Entity<GunUpgradeComponent>>();

        for (var i = 0; i < ent.Comp.MaxUpgradeCount; i++)
        {
            var slotId = $"{ent.Comp.UpgradesContainerId}-{i}";
            if (_itemSlots.GetItemOrNull(ent, slotId) is { } item &&
                TryComp<GunUpgradeComponent>(item, out var upgradeComp))
            {
                upgrades.Add((item, upgradeComp));
            }
        }

        return upgrades;
    }

    public IEnumerable<ProtoId<TagPrototype>> GetCurrentUpgradeTags(Entity<UpgradeableGunComponent> ent)
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            foreach (var tag in upgrade.Comp.Tags)
            {
                yield return tag;
            }
        }
    }
}
