using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee;


namespace Content.Server._pofitlo.CombatExtended.FightAction;

public sealed class FightActionSystem : SharedFightActionSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FightActionChangeEvent>(OnFightActionChange);
    }

    private void OnFightActionChange(FightActionChangeEvent message, EntitySessionEventArgs args)
    {
        if (!TryComp<FightActionComponent>(GetEntity(message.Uid), out var fightActionComp))
            return;

        if (args.SenderSession.AttachedEntity != GetEntity(message.Uid))
            return; //That help us avoid desync issues, as the client will only send this event for their own entity, but we check just in case.

        var uid = GetEntity(message.Uid);

        fightActionComp.Strategy = message.FightAction;
        fightActionComp.HasHigherPriorityThanWeapons = message.HasHigherPriorityThanWeapons;
        fightActionComp.CombatAnimationPrototype = message.CombatAnimationProto;
        fightActionComp.AltCombatAnimationPrototype = message.AltCombatAnimationProto;
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.Strategy));
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.HasHigherPriorityThanWeapons));
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.CombatAnimationPrototype));
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.AltCombatAnimationPrototype));

        ApplyMeleeParameters(uid, fightActionComp, message.FightAction);
    }

    /// <summary>
    ///     Applies the per-entity melee parameters declared for the selected strategy to the entity's
    ///     <see cref="MeleeWeaponComponent"/>. Parameters live on the entity prototype, because different
    ///     races have different base attacks.
    /// </summary>
    private void ApplyMeleeParameters(EntityUid user, FightActionComponent fightActionComp, AttackStrategy strategy)
    {
        if (!TryComp<MeleeWeaponComponent>(user, out var meleeComp) ||
            !fightActionComp.MeleeParameters.TryGetValue(strategy, out var parameters))
            return;

        meleeComp.AltDisarm = parameters.HasDisarm;
        DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.AltDisarm));

        // Every combat field is optional: only the ones declared on the entity override the weapon.
        if (parameters.Damage is { } damage)
        {
            // Copy so we never mutate the parameters instance the component holds.
            meleeComp.Damage = new DamageSpecifier(damage);
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.Damage));
        }

        if (parameters.AttackRate is { } attackRate)
        {
            meleeComp.AttackRate = attackRate;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.AttackRate));
        }

        if (parameters.Range is { } range)
        {
            meleeComp.Range = range;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.Range));
        }

        if (parameters.Angle is { } angle)
        {
            meleeComp.Angle = angle;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.Angle));
        }

        if (parameters.ClickDamageModifier is { } clickDamageModifier)
        {
            meleeComp.ClickDamageModifier = clickDamageModifier;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.ClickDamageModifier));
        }

        if (parameters.BluntStaminaDamageFactor is { } bluntStaminaDamageFactor)
        {
            meleeComp.BluntStaminaDamageFactor = bluntStaminaDamageFactor;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.BluntStaminaDamageFactor));
        }

        if (parameters.HeavyStaminaCost is { } heavyStaminaCost)
        {
            meleeComp.HeavyStaminaCost = heavyStaminaCost;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.HeavyStaminaCost));
        }

        if (parameters.CanHeavyAttack is { } canHeavyAttack)
        {
            meleeComp.CanHeavyAttack = canHeavyAttack;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.CanHeavyAttack));
        }

        if (parameters.CanWideSwing is { } canWideSwing)
        {
            meleeComp.CanWideSwing = canWideSwing;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.CanWideSwing));
        }

        if (parameters.ResistanceBypass is { } resistanceBypass)
        {
            meleeComp.ResistanceBypass = resistanceBypass;
            DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.ResistanceBypass));
        }
    }
}
