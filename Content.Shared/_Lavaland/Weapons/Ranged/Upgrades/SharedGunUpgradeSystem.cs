using Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared.CCVar;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Configuration;
using System.Linq;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades;

public abstract partial class SharedGunUpgradeSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UpgradeableGunComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<UpgradeableGunComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttemptEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<UpgradeableGunComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UpgradeableGunComponent, GunRefreshModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, GunShotEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableGunComponent, ProjectileShotEvent>(RelayEvent);
        SubscribeLocalEvent<GunUpgradeComponent, ExaminedEvent>(OnUpgradeExamine);
        SubscribeLocalEvent<GunUpgradeFireRateComponent, GunRefreshModifiersEvent>(OnFireRateRefresh);
        SubscribeLocalEvent<GunComponentUpgradeComponent, GunRefreshModifiersEvent>(OnCompsRefresh);
        SubscribeLocalEvent<GunUpgradeSpeedComponent, GunRefreshModifiersEvent>(OnSpeedRefresh);
        SubscribeLocalEvent<GunUpgradeComponentsComponent, GunShotEvent>(OnDamageGunShotComps);
        SubscribeLocalEvent<GunUpgradeVampirismComponent, GunShotEvent>(OnVampirismGunShot);
        SubscribeLocalEvent<ProjectileVampirismComponent, ProjectileHitEvent>(OnVampirismProjectileHit);
    }

    private void OnMapInit(EntityUid uid, UpgradeableGunComponent component, MapInitEvent args)
    {
        var itemSlots = EnsureComp<ItemSlotsComponent>(uid);
        CreateNewSlot(uid, component, itemSlots);
    }

    private void CreateNewSlot(EntityUid uid, UpgradeableGunComponent component, ItemSlotsComponent? slotComp = null)
    {
        if (!Resolve(uid, ref slotComp))
            return;

        var slotCount = slotComp.Slots.Keys.Count(slotId => slotId.StartsWith(component.UpgradesContainerId));
        var slotId = $"{component.UpgradesContainerId}-{slotCount + 1}";
        var slot = new ItemSlot
        {
            Whitelist = component.Whitelist,
            Swap = false,
            EjectOnBreak = true,
            Name = Loc.GetString("upgradeable-gun-slot-name", ("value", slotCount + 1))
        };

        _itemSlots.AddItemSlot(uid, slotId, slot, slotComp);
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
        int usedCapacity = 0;
        using (args.PushGroup(nameof(UpgradeableGunComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                args.PushMarkup(Loc.GetString(upgrade.Comp.ExamineText));
                usedCapacity += upgrade.Comp.CapacityCost;
            }
            args.PushMarkup(Loc.GetString("upgradeable-gun-total-remaining-capacity", ("value", ent.Comp.MaxUpgradeCapacity - usedCapacity)));
        }
    }

    private void OnUpgradeExamine(Entity<GunUpgradeComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineText));
        args.PushMarkup(Loc.GetString("gun-upgrade-examine-text-capacity-cost", ("value", ent.Comp.CapacityCost)));
    }

    private void OnInteractUsing(Entity<UpgradeableGunComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled
            || !HasComp<GunUpgradeComponent>(args.Used)
            || !TryComp<ItemSlotsComponent>(ent, out var itemSlots)
            || _entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Used))
            return;

        var currentUpgrades = GetCurrentUpgrades(ent, itemSlots);

        // Create a new slot if all current slots are filled
        if (currentUpgrades.Count + 1 > itemSlots.Slots.Keys.Count(slotId =>
            slotId.StartsWith(ent.Comp.UpgradesContainerId)))
            CreateNewSlot(ent, ent.Comp);

        if (_itemSlots.TryInsertEmpty((ent.Owner, itemSlots), args.Used, args.User, true))
            _gun.RefreshModifiers(ent.Owner);

        args.Handled = true;
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

        var allowDupes = _config.GetCVar(CCVars.AllowDuplicatePkaModules);
        var itemProto = MetaData(args.Item).EntityPrototype?.ID;
        foreach (var itemSlot in itemSlots.Slots.Values)
        {
            if (itemSlot.HasItem
                && itemSlot.Item is { } existingItem
                && MetaData(existingItem).EntityPrototype?.ID == itemProto
                && !allowDupes)
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

    private void OnCompsRefresh(Entity<GunComponentUpgradeComponent> ent, ref GunRefreshModifiersEvent args)
    {
        EntityManager.AddComponents(args.Gun, ent.Comp.Components);
    }

    private void OnSpeedRefresh(Entity<GunUpgradeSpeedComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.ProjectileSpeed *= ent.Comp.Coefficient;
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

    public HashSet<Entity<GunUpgradeComponent>> GetCurrentUpgrades(Entity<UpgradeableGunComponent> ent, ItemSlotsComponent? itemSlots = null)
    {
        if (!Resolve(ent, ref itemSlots))
            return new HashSet<Entity<GunUpgradeComponent>>();

        var upgrades = new HashSet<Entity<GunUpgradeComponent>>();

        foreach (var itemSlot in itemSlots.Slots.Values)
        {
            if (itemSlot.HasItem
                && itemSlot.Item is { } item
                && TryComp<GunUpgradeComponent>(item, out var upgradeComp))
                upgrades.Add((item, upgradeComp));
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
