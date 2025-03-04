using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Goobstation.MartialArts;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedMartialArtsSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;

    private void InitializeKravMaga()
    {
        SubscribeLocalEvent<GrantKravMagaComponent, ClothingGotEquippedEvent>(OnGlovesGotEquipped);
        SubscribeLocalEvent<GrantKravMagaComponent, ClothingGotUnequippedEvent>(OnGlovesGotUnequipped);
        SubscribeLocalEvent<KravMagaComponent, KravMagaActionEvent>(OnKravMagaAction);
        SubscribeLocalEvent<KravMagaComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }
    private void OnMeleeHitEvent(Entity<KravMagaComponent> ent, ref MeleeHitEvent args)
    {
        if(args.HitEntities.Count <= 0)
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hitEntity))
                continue;
            if (!TryComp<RequireProjectileTargetComponent>(hitEntity, out var isDowned))
                continue;

            DoKravMaga(ent, hitEntity, isDowned.Active);
        }
    }

    private void DoKravMaga(Entity<KravMagaComponent> ent, EntityUid hitEntity, bool active)
    {
        if(ent.Comp.SelectedMoveComp == null)
            return;
        var moveComp = ent.Comp.SelectedMoveComp;

        switch (ent.Comp.SelectedMove)
        {
            case KravMagaMoves.LegSweep:
                _stun.TryParalyze(hitEntity, TimeSpan.FromSeconds(4), true);
                break;
            case KravMagaMoves.NeckChop:
                DoDamage(ent.Owner, hitEntity, ent.Comp.BaseDamage, "Blunt");

                var comp = EnsureComp<KravMagaSilencedComponent>(hitEntity);
                comp.SilencedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.effectTime);
                break;
            case KravMagaMoves.LungPunch:
                _stamina.TryTakeStamina(hitEntity, moveComp.staminaDamage);
                var blockedBreathingComponent = EnsureComp<KravMagaBlockedBreathingComponent>(hitEntity);
                blockedBreathingComponent.BlockedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.effectTime);
                break;
            case null:
                DoDamage(ent.Owner, hitEntity, ent.Comp.BaseDamage, "Blunt", active);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        ent.Comp.SelectedMove = null;
        ent.Comp.SelectedMoveComp = null;
    }

    private void DoDamage(EntityUid uid, EntityUid target, int amount, string type,bool? extraDamageOnGround = null)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.Add(type, amount);
        if (extraDamageOnGround is true)
            damage *= 2;
        _damageable.TryChangeDamage(target, damage, origin: uid);
    }

    private void OnKravMagaAction(Entity<KravMagaComponent> ent, ref KravMagaActionEvent args)
    {
        var actionEnt = args.Action.Owner;
        if(!TryComp<KravMagaActionComponent>(actionEnt, out var kravActionComp))
            return;

        _popupSystem.PopupClient(Loc.GetString("krav-maga-ready", ("action", kravActionComp.Name)), ent, ent);
        ent.Comp.SelectedMove = kravActionComp.Configuration;
        ent.Comp.SelectedMoveComp = kravActionComp;
    }

    private void OnGlovesGotEquipped(Entity<GrantKravMagaComponent> ent, ref ClothingGotEquippedEvent args)
    {
        var kravMaga = EnsureComp<KravMagaComponent>(args.Wearer);
        foreach (var actionId in kravMaga.BaseKravMagaMoves)
        {
            var actions = _actions.AddAction(args.Wearer, actionId);
            if (actions != null)
                kravMaga.KravMagaMoveEntities.Add(actions.Value);
        }
    }

    private void OnGlovesGotUnequipped(Entity<GrantKravMagaComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!TryComp<KravMagaComponent>(args.Wearer, out var kravMaga))
            return;

        foreach (var action in kravMaga.KravMagaMoveEntities)
        {
            _actions.RemoveAction(action);
        }
        RemComp<KravMagaComponent>(args.Wearer);
    }
}
