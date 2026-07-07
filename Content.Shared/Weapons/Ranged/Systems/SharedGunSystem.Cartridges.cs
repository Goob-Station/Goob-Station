// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private readonly DamageExamineSystem _damageExamine = default!;

    // needed for server system
    protected virtual void InitializeCartridge()
    {
        SubscribeLocalEvent<CartridgeAmmoComponent, ExaminedEvent>(OnCartridgeExamine);
        SubscribeLocalEvent<CartridgeAmmoComponent, DamageExamineEvent>(OnCartridgeDamageExamine);
        SubscribeLocalEvent<BasicEntityAmmoProviderComponent, DamageExamineEvent>(OnBasicEntityDamageExamine); // Goobstation
    }

    private void OnCartridgeExamine(Entity<CartridgeAmmoComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(ent.Comp.Spent
            ? Loc.GetString("gun-cartridge-spent")
            : Loc.GetString("gun-cartridge-unspent"));
    }

    private void OnCartridgeDamageExamine(Entity<CartridgeAmmoComponent> ent, ref DamageExamineEvent args)
    {
        var damageSpec = GetProjectileDamage(ent.Comp.Prototype);

        if (damageSpec == null)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-projectile"));

        // Goobstation START - partial armor penetration
        var ap = GetProjectilePenetration(ent.Comp.Prototype);
        if (ap == 0)
            return;
        var abs = Math.Abs(ap);
        args.Message.AddMarkupPermissive("\n" + Loc.GetString("armor-penetration", ("arg", ap/abs), ("abs", abs)));
        // Goobstation END
    }

    private DamageSpecifier? GetProjectileDamage(EntProtoId proto)
    {
        if (!ProtoManager.TryIndex(proto, out var entityProto))
            return null;

        if (!entityProto.TryGetComponent<ProjectileComponent>(out var projectile, Factory))
            return null;

        if (!projectile.Damage.Empty)
            return projectile.Damage * Damageable.UniversalProjectileDamageModifier;

        return null;
    }
    // Goobstation start - partial armor penetration
    private void OnBasicEntityDamageExamine(EntityUid uid, BasicEntityAmmoProviderComponent component, ref DamageExamineEvent args)
    {
        if (component.Proto == null)
            return;

        var damageSpec = GetProjectileDamage(component.Proto);

        if (damageSpec == null)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-projectile"));

        var ap = GetProjectilePenetration(component.Proto);
        if (ap == 0)
            return;

        var abs = Math.Abs(ap);
        args.Message.AddMarkupPermissive("\n" + Loc.GetString("armor-penetration", ("arg", ap/abs), ("abs", abs)));
    }
    public int GetProjectilePenetration(string proto)
    {
        if (!ProtoManager.TryIndex<EntityPrototype>(proto, out var entityProto)
            || !entityProto.Components.TryGetValue(Factory.GetComponentName<ProjectileComponent>(), out var projectile))
            return 0;

        var p = (ProjectileComponent) projectile.Component;

        return p.IgnoreResistances ? 100 : (int)Math.Round(p.Damage.ArmorPenetration * 100);
    }
    // Goobstation end
}
