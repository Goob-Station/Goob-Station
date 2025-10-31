using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed class RallySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _status = default!;

    private readonly HashSet<Entity<MeleeWeaponComponent>> _melee = new();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RallyComponent, RallyEvent>(OnRally);

        SubscribeLocalEvent<RalliedComponent, StatusEffectAppliedEvent>(OnEffectApplied);
        SubscribeLocalEvent<RalliedComponent, StatusEffectRemovedEvent>(OnEffectRemoved);
    }

    private void OnRally(Entity<RallyComponent> ent, ref RallyEvent args)
    {
        // Get all entities in range of the rally
        _melee.Clear();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.RallyRange, _melee);

        foreach (var affected in _melee)
        {
            if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, affected))
                continue;

            _status.TryAddStatusEffect(affected, ent.Comp.StatusEffectRally, out _, ent.Comp.Duration);

            _popup.PopupClient(Loc.GetString("wraith-skeleton-rally-howl", ("user", ent.Owner)), ent.Owner, ent.Owner);
        }

        args.Handled = true;
    }

    private void OnEffectApplied(Entity<RalliedComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<MeleeWeaponComponent>(args.Target, out var melee))
            return;

        // save original damage
        ent.Comp.OriginalDamage = melee.Damage;
        ent.Comp.OriginalSpeed = melee.AttackRate;
        Dirty(ent);

        // apply buffs
        melee.Damage *= ent.Comp.RalliedStrength;
        melee.AttackRate *= ent.Comp.RalliedAttackSpeed;
        Dirty(args.Target, melee);
    }

    private void OnEffectRemoved(Entity<RalliedComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TryComp<MeleeWeaponComponent>(args.Target, out var melee)
            || ent.Comp.OriginalDamage == null)
            return;

        melee.Damage = ent.Comp.OriginalDamage;
        melee.AttackRate = ent.Comp.OriginalSpeed;
        Dirty(args.Target, melee);
    }
}
