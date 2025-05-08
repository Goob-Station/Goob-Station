// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.EnsareTimeModifier;
using Content.Goobstation.Common.Wizard.Mutate;
using Content.Shared.Cuffs;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Slippery;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Mutate;

public abstract class SharedHulkSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HulkComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<HulkComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<HulkComponent, SlipAttemptEvent>(OnSlipAttempt);
        SubscribeLocalEvent<HulkComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<HulkComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HulkComponent, GetEnsareTimeModifier>(OnGetEnsareTimeModifier);
        SubscribeLocalEvent<HulkComponent, HulkUncuffAttemptEvent>(OnUncuffAttemptEvent);
    }

    private void OnUncuffAttemptEvent(Entity<HulkComponent> ent, ref HulkUncuffAttemptEvent args)
    {
        Roar(ent);
        _cuffable.Uncuff(ent,ent, args.cuffs);
        args.Success = true;
    }

    private void OnGetEnsareTimeModifier(Entity<HulkComponent> ent, ref GetEnsareTimeModifier args)
    {
        Roar(ent);
        args.FreeTime *= ent.Comp.EnsnareTimeMultiplier;
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

    private void OnBeforeStatusEffect(Entity<HulkComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (args.Key is not ("KnockedDown" or "Stun"))
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
