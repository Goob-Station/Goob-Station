using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Slippery;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeBlade()
    {
        // Protective blades prevent that
        // SubscribeLocalEvent<SilverMaelstromComponent, BeforeStaminaDamageEvent>(OnBeforeBladeStaminaDamage);
        // Still knocked down by a flashbang or something - it's fine
        // SubscribeLocalEvent<SilverMaelstromComponent, BeforeStatusEffectAddedEvent>(OnBeforeBladeStatusEffect);
        // Remove this after adding noslip heretic magboots side knowledge
        SubscribeLocalEvent<SilverMaelstromComponent, SlipAttemptEvent>(OnBladeSlipAttempt);
        // Protective blades do that
        // SubscribeLocalEvent<SilverMaelstromComponent, BeforeHarmfulActionEvent>(OnBladeHarmfulAction);

        SubscribeLocalEvent<RealignmentComponent, BeforeStaminaDamageEvent>(OnBeforeBladeStaminaDamage);
        SubscribeLocalEvent<RealignmentComponent, BeforeStatusEffectAddedEvent>(OnBeforeBladeStatusEffect);
        SubscribeLocalEvent<RealignmentComponent, SlipAttemptEvent>(OnBladeSlipAttempt);
        SubscribeLocalEvent<RealignmentComponent, BeforeHarmfulActionEvent>(OnBladeHarmfulAction);
        SubscribeLocalEvent<RealignmentComponent, StatusEffectEndedEvent>(OnStatusEnded);
    }

    private void OnStatusEnded(Entity<RealignmentComponent> ent, ref StatusEffectEndedEvent args)
    {
        if (args.Key != "Pacified")
            return;

        if (!_status.TryRemoveStatusEffect(ent, "Realignment"))
            RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnBladeHarmfulAction(EntityUid uid, Component component, BeforeHarmfulActionEvent args)
    {
        if (args.Cancelled || args.Type == HarmfulActionType.Harm)
            return;

        args.Cancel();
    }

    private void OnBladeSlipAttempt(EntityUid uid, Component component, SlipAttemptEvent args)
    {
        args.NoSlip = true;
    }

    private void OnBeforeBladeStatusEffect(EntityUid uid, Component component, ref BeforeStatusEffectAddedEvent args)
    {
        if (args.Key is not ("KnockedDown" or "Stun"))
            return;

        args.Cancelled = true;
    }

    private void OnBeforeBladeStaminaDamage(EntityUid uid, Component component, ref BeforeStaminaDamageEvent args)
    {
        args.Cancelled = true;
    }
}
