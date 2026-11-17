using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts.Specific;

/// <summary>
/// Code-wise this is essentially nothing like the rest of martial arts and should be treated as such
/// </summary>
public sealed partial class KravMagaSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private StandingStateSystem _standingState = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    private void DoKravMaga(
        Entity<KravMagaComponent> ent,
        EntityUid hitEntity, RequireProjectileTargetComponent reguireProjectileTargetComponent)
    {

        if (ent.Comp.SelectedMoveComp is not { } moveComp)
            return;

        switch (ent.Comp.SelectedMove)
        {
            case KravMagaMoves.LegSweep:
                if (_standingState.IsDown(hitEntity))
                    break;
                _stun.TryKnockdown(hitEntity, TimeSpan.FromSeconds(4), true); // okay buddy
                // _stamina.TakeStaminaDamage(hitEntity, moveComp.StaminaDamage, applyResistances: true);
                break;
            case KravMagaMoves.NeckChop:
                var comp = EnsureComp<KravMagaSilencedComponent>(hitEntity);
                comp.SilencedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.EffectTime);
                break;
            case KravMagaMoves.LungPunch:
                _stamina.TakeStaminaDamage(hitEntity, moveComp.StaminaDamage);
                var blockedBreathingComponent = EnsureComp<KravMagaBlockedBreathingComponent>(hitEntity);
                blockedBreathingComponent.BlockedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.EffectTime);
                break;
            case null:
                var damage = ent.Comp.BaseDamage;
                if (reguireProjectileTargetComponent.Active)
                    damage *= ent.Comp.DownedDamageModifier;

                var dmgSpec = new DamageSpecifier();
                dmgSpec.DamageDict["Blunt"] = damage;
                _damageable.TryChangeDamage(ent.Owner, dmgSpec);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ent.Comp.SelectedMove = null;
        ent.Comp.SelectedMoveComp = null;
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnKravMagaAction(Entity<KravMagaComponent> ent, ref KravMagaActionEvent args)
    {
        var actionEnt = args.Action.Owner;
        if (!TryComp<KravMagaActionComponent>(actionEnt, out var kravActionComp))
            return;

        args.Handled = true;

        _popup.PopupClient(Loc.GetString("krav-maga-ready", ("action", kravActionComp.Name)), ent, ent);
        ent.Comp.SelectedMove = kravActionComp.Configuration;
        ent.Comp.SelectedMoveComp = kravActionComp;
    }

    [SubscribeLocalEvent]
    private void OnMeleeHitEvent(Entity<KravMagaComponent> ent, ref MeleeHitEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (args.HitEntities.Count <= 0)
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hitEntity))
                continue;
            if (!TryComp<RequireProjectileTargetComponent>(hitEntity, out var isDowned))
                continue;

            DoKravMaga(ent, hitEntity, isDowned);
        }
    }
}
