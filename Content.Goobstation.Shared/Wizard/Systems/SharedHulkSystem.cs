// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Wizard.Events;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Slippery;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems;

public abstract class SharedHulkSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HulkComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<HulkComponent, BeforeOldStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<HulkComponent, SlipAttemptEvent>(OnSlipAttempt);
        SubscribeLocalEvent<HulkComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<HulkComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HulkComponent, UncuffAttemptEvent>(OnUncuffAttempt);
        SubscribeLocalEvent<HulkComponent, EnsnareableModifyDurationEvent>(OnModifyEnsnareableDuration);
        SubscribeLocalEvent<HulkComponent, EnsnareRemoveEvent>(OnEnsnareRemove);
    }

    private void OnModifyEnsnareableDuration(Entity<HulkComponent> ent, ref EnsnareableModifyDurationEvent args)
    {
        args.Duration = 0;
    }

    private void OnEnsnareRemove(Entity<HulkComponent> ent, ref EnsnareRemoveEvent args)
    {
        Roar(ent);
    }

    private void OnUncuffAttempt(Entity<HulkComponent> ent, ref UncuffAttemptEvent args)
    {
        if (args.Target != args.User || !_cuffable.TryGetLastCuff(args.User, out var cuff))
            return;

        Roar(ent);
        _cuffable.Uncuff(args.User, args.User, cuff.Value);

        args.Cancelled = true;
    }

    private void OnStartup(Entity<HulkComponent> ent, ref ComponentStartup args)
    {
        UpdateColorStartup(ent);
        ent.Comp.StructuralDamage ??= new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Structural"), 80f);
    }

    private void OnMeleeHit(Entity<HulkComponent> ent, ref MeleeHitEvent args)
    {
        args.BonusDamage += args.BaseDamage * ent.Comp.FistDamageMultiplier;
        var total = args.BonusDamage.GetTotal();
        if (total > 0 && total > ent.Comp.MaxBonusFistDamage)
            args.BonusDamage *= ent.Comp.MaxBonusFistDamage / total;

        if (ent.Comp.StructuralDamage != null)
            args.BonusDamage += ent.Comp.StructuralDamage;

        if (args.HitEntities.Count > 0)
            Roar(ent, 0.2f);
    }

    private void OnSlipAttempt(Entity<HulkComponent> ent, ref SlipAttemptEvent args)
    {
        args.NoSlip = true;
    }

    private void OnBeforeStatusEffect(Entity<HulkComponent> ent, ref BeforeOldStatusEffectAddedEvent args)
    {
        if (args.EffectKey is not ("KnockedDown" or "Stun"))
            return;

        Roar(ent);
        args.Cancelled = true;
    }

    private void OnBeforeStaminaDamage(Entity<HulkComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        args.Cancelled = true;
    }

    protected virtual void UpdateColorStartup(Entity<HulkComponent> hulk)
    {
    }

    public virtual void Roar(Entity<HulkComponent> hulk, float prob = 1f)
    {
    }
}
