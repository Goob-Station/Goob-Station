using Content.Shared._Goobstation.MartialArts;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Content.Shared._Goobstation.MartialArts.Components;

namespace Content.Server._Goobstation.MartialArts;

public sealed partial class MartialArtsSystem
{
    private void InitializeCqc()
    {
        SubscribeLocalEvent<CanPerformComboComponent, CQCSlamPerformedEvent>(OnCQCSlam);
        SubscribeLocalEvent<CanPerformComboComponent, CQCKickPerformedEvent>(OnCQCKick);
        SubscribeLocalEvent<CanPerformComboComponent, CQCRestrainPerformedEvent>(OnCQCRestrain);
        SubscribeLocalEvent<CanPerformComboComponent, CQCPressurePerformedEvent>(OnCQCPressure);
        SubscribeLocalEvent<CanPerformComboComponent, CQCConsecutivePerformedEvent>(OnCQCConsecutive);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComboAttackPerformedEvent>(OnCQCAttackPerformed);
        SubscribeLocalEvent<GrantCqcComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCqcComponent, ExaminedEvent>(OnGrantCQCExamine);
    }

    #region Generic Methods

    private void OnGrantCQCUse(Entity<GrantCqcComponent> ent, ref UseInHandEvent args)
    {
        if (ent.Comp.Used)
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-used", ("manual", Identity.Entity(ent, EntityManager))),
                args.User,
                args.User);
            return;
        }

        if (!TryGrant(ent.Comp, args.User))
            return;
        _popupSystem.PopupEntity(Loc.GetString("cqc-success-learned"), args.User, args.User);
        ent.Comp.Used = true;
    }

    private void OnGrantCQCExamine(Entity<GrantCqcComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Used)
            args.PushMarkup(Loc.GetString("cqc-manual-used", ("manual", Identity.Entity(ent, EntityManager))));
    }

    private void OnCQCAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (!TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledgeComponent))
            return;

        if (!knowledgeComponent.Blocked)
            return;

        if (knowledgeComponent.MartialArtsForm != MartialArtsForms.CloseQuartersCombat)
            return;

        if (args.Type != ComboAttackType.Disarm)
            return;
        if (!_random.Prob(0.5f)) // random chance to steal items? this
            return;

        var item = _hands.GetActiveItem(args.Target);

        if (item == null)
            return;
        _hands.TryDrop(args.Target, item.Value);
        _hands.TryPickupAnyHand(ent, item.Value);
        _stamina.TakeStaminaDamage(args.Target, 10f);
    }

    #endregion

    #region Combo Methods

    private void OnCQCSlam(Entity<CanPerformComboComponent> ent, ref CQCSlamPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out var downed))
            return;

        if (downed)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stun.TryParalyze(target, TimeSpan.FromSeconds(12), true);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }

    private void OnCQCKick(Entity<CanPerformComboComponent> ent, ref CQCKickPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out var downed))
            return;

        if (!downed)
            return;

        if (TryComp<StaminaComponent>(target, out var stamina) && stamina.Critical)
        {
            _status.TryAddStatusEffect<ForcedSleepingComponent>(target, "ForcedSleep", TimeSpan.FromSeconds(10), true);
        }

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stamina.TakeStaminaDamage(target, 55f, source: ent);

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 1f / dir.Length();
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, dir, 25f, damage, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
    }

    private void OnCQCRestrain(Entity<CanPerformComboComponent> ent, ref CQCRestrainPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out _))
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(10), true);
        _stamina.TakeStaminaDamage(target, 30f, source: ent);
    }

    private void OnCQCPressure(Entity<CanPerformComboComponent> ent, ref CQCPressurePerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out _))
            return;

        if (!_hands.TryGetActiveItem(target, out var activeItem))
            return;
        _hands.TryDrop(target, activeItem.Value);
        _hands.TryPickupAnyHand(ent, activeItem.Value);
        _stamina.TakeStaminaDamage(target, 65f, source: ent);
    }

    private void OnCQCConsecutive(Entity<CanPerformComboComponent> ent, ref CQCConsecutivePerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out _))
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 20);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stamina.TakeStaminaDamage(target, 70, source: ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
    }

    #endregion
}
