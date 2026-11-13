using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.MartialArts;

public sealed partial class MartialArtsSystem
{
    [SubscribeLocalEvent]
    private void OnMartialArtKnowledgeAdded(
        Entity<MartialArtsKnowledgeComponent> ent,
        ref MartialArtKnowledgeAdded args)
    {
        EntityManager.AddComponents(ent, args.MartialArt.ComponentGrant);
        EnsureComp<ComboPerformerComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void OnMartialArtKnowledgeRemoved(
        Entity<MartialArtsKnowledgeComponent> ent,
        ref MartialArtKnowledgeRemoved args)
    {
        EntityManager.RemoveComponents(ent, args.MartialArt.ComponentGrant);

        if (ent.Comp.Knowledge.Count > 0)
            return;

        // shutdown and cleaup martial art stuff if we no longer know any of them
        RemComp<MartialArtsKnowledgeComponent>(ent);
        RemComp<ComboPerformerComponent>(ent); // todo separate?
    }

    [SubscribeLocalEvent]
    private void OnDiscoverCombos(Entity<MartialArtsKnowledgeComponent> ent, ref GetAvailableCombosEvent args)
    {
        foreach (var art in SortByWeights(GetActiveMartialArts(ent.AsNullable())))
        {
            var ev = new MartialArtsAttemptEvent(art);
            RaiseLocalEvent(ev);
            if (ev.Cancelled)
                continue;
            args.Combos.AddRange(art.Combos);
        }
    }

    [SubscribeLocalEvent]
    private void OnGetMeleeDamage(Entity<MartialArtsKnowledgeComponent> ent, ref GetMeleeDamageEvent args)
    {
        var arts = SortByWeights(GetMartialArts(ent.AsNullable()));
        var art = arts[0]; // we use the highest priority martial art for damage buffs

        if (TryComp<MartialArtModifiersComponent>(ent, out var modifiers))
        {
            var (mult, mod) =
                GetMultiplierModifier((ent, modifiers), MartialArtModifierType.Damage, args.Weapon != ent.Owner);
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, GetDamageModifierSet(args.Damage, mult, mod));
        }

        args.Damage *= art.FistDamageMultiplier;
    }

    [SubscribeLocalEvent]
    private void OnBeforeStatusStamina(Entity<StatusEffectContainerComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (!_status.TryEffectsWithComp<StaminaResistanceModifierStatusEffectComponent>(ent, out var effects))
            return;

        foreach (var effect in effects)
        {
            args.Value *= effect.Comp1.Modifier;
        }
    }

    [SubscribeLocalEvent]
    private void OnGetMovespeed(Entity<MartialArtModifiersComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var (mult, _) = GetMultiplierModifier(ent, MartialArtModifierType.MoveSpeed, null);
        args.ModifySpeed(mult, mult);
    }

    [SubscribeLocalEvent]
    private void OnBlockedMeleeHit(Entity<MartialArtBlockedComponent> ent, ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!ent.Comp.StaminaDmgBlock)
            return;

        if (!HasMartialArt(ent.Owner, _proto.Index(ent.Comp.Form)))
            return;

        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnGetMeleeAttackRate(Entity<MartialArtModifiersComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        var (mult, mod) = GetMultiplierModifier(ent, MartialArtModifierType.AttackRate, args.Weapon != args.User);
        args.Multipliers *= mult;
        args.Rate += mod;
    }

    /*[SubscribeLocalEvent] TODO: readd this as another component or a status effect lmao why was it here
    private void CheckGrabStageOverride<T>(Entity<MartialArtsKnowledgeComponent> ent, ref CheckGrabOverridesEvent args)
    {
        if (args.Stage == GrabStage.Soft)
            args.Stage = ent.Comp.StartingStage;
    }*/
}
