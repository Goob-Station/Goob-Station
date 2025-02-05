using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;
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

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UpgradeableGunComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<UpgradeableGunComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<UpgradeableGunComponent, ExaminedEvent>(OnExamine);

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

    private void OnStartup(Entity<UpgradeableGunComponent> ent, ref ComponentStartup args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.UpgradesContainerId);
    }

    private void OnAfterInteractUsing(Entity<UpgradeableGunComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach || !HasComp<GunUpgradeComponent>(args.Used))
            return;

        if (GetCurrentUpgrades(ent).Count >= ent.Comp.MaxUpgradeCount)
        {
            _popup.PopupPredicted(Loc.GetString("upgradeable-gun-popup-upgrade-limit"), ent, args.User);
            return;
        }

        if (_entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Used))
            return;

        _audio.PlayPredicted(ent.Comp.InsertSound, ent, args.User);
        _gun.RefreshModifiers(ent.Owner);
        args.Handled = _container.Insert(args.Used, _container.GetContainer(ent, ent.Comp.UpgradesContainerId));
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
        if (!_container.TryGetContainer(ent, ent.Comp.UpgradesContainerId, out var container))
            return new HashSet<Entity<GunUpgradeComponent>>();

        var upgrades = new HashSet<Entity<GunUpgradeComponent>>();
        foreach (var contained in container.ContainedEntities)
        {
            if (TryComp<GunUpgradeComponent>(contained, out var upgradeComp))
                upgrades.Add((contained, upgradeComp));
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
