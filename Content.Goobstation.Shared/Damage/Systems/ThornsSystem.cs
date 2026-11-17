using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Network;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Damage;

/// <summary>
/// Upstream has their own version of thorns but it does not accept ranged damage and only deals flat damage.
/// Why not touch upstream? Well, blame Marty.
/// </summary>

public sealed partial class ThornsSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    private bool _reflecting;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThornsComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<ThornsComponent, ProjectileReflectAttemptEvent>(OnProjectile);
        SubscribeLocalEvent<ThornsComponent, HitScanReflectAttemptEvent>(OnHitscan);
    }

    private void OnAttacked(Entity<ThornsComponent> ent, ref AttackedEvent args)
    {
        if (!ent.Comp.Melee)
            return;

        Reflect(ent, args.User, _melee.GetDamage(args.Used, args.User));
    }

    private void OnProjectile(Entity<ThornsComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (!ent.Comp.Ranged || args.Cancelled)
            return;

        if (!TryComp<ProjectileComponent>(args.ProjUid, out var projectile) || projectile.Shooter is not { } shooter)
            return;

        Reflect(ent, shooter, projectile.Damage);
    }

    private void OnHitscan(Entity<ThornsComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (!ent.Comp.Ranged || args.Reflected)
            return;

        if (args.Shooter is not { } shooter)
            return;

        Reflect(ent, shooter, args.Damage);
    }

    private void Reflect(Entity<ThornsComponent> ent, EntityUid attacker, DamageSpecifier? dealt)
    {
        if (!_net.IsServer || _reflecting || attacker == ent.Owner)
            return;

        if (!_mobState.IsAlive(attacker))
            return;

        var reflected = new DamageSpecifier(ent.Comp.FlatDamage);

        if (ent.Comp.DamageFraction > 0f && dealt is not null)
        {
            reflected += DamageSpecifier.GetPositive(dealt) * ent.Comp.DamageFraction;
        }

        if (reflected.Empty)
            return;

        if (!ent.Comp.IgnoreResistances && GetProtection(attacker) is { } protection)
        {
            reflected = DamageSpecifier.ApplyModifierSet(reflected, protection);
        }

        // so we dont get a death loop between two thorn having enttities
        _reflecting = true;
        var dealtBack = _damageable.ChangeDamage(attacker, reflected, ent.Comp.IgnoreResistances, origin: ent.Owner, canBeCancelled: true);
        _reflecting = false;

        if (!dealtBack.AnyPositive())
            return;

        _adminLogger.Add(LogType.Damaged, $"{ToPrettyString(attacker):user} was hurt by thorns on {ToPrettyString(ent):target} and received {dealtBack.GetTotal():damage} damage");

        if (ent.Comp.Sound is not null)
        {
            _audio.PlayPvs(ent.Comp.Sound, attacker);
        }

        _popup.PopupEntity(Loc.GetString(ent.Comp.PopupText), attacker, attacker);
    }

    private DamageModifierSet? GetProtection(EntityUid attacker)
    {
        _inventory.TryGetInventoryEntity<DamageOnAttackedProtectionComponent>(attacker, out var protective);

        if (protective.Comp is null && TryComp<HandsComponent>(attacker, out var hands) && _hands.TryGetActiveItem((attacker, hands), out var item) && TryComp<DamageOnAttackedProtectionComponent>(item, out var itemProtection) && itemProtection.Slots == SlotFlags.NONE)
        {
            protective = (item.Value, itemProtection);
        }

        if (protective.Comp is null && TryComp<DamageOnAttackedProtectionComponent>(attacker, out var selfProtection))
        {
            protective = (attacker, selfProtection);
        }

        return protective.Comp?.DamageProtection;
    }
}
