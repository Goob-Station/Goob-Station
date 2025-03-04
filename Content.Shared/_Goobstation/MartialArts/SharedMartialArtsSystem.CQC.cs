using Content.Shared._Goobstation.MartialArts.Components;
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

namespace Content.Shared._Goobstation.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeCqc()
    {
        SubscribeLocalEvent<CanPerformComboComponent, CqcSlamPerformedEvent>(OnCQCSlam);
        SubscribeLocalEvent<CanPerformComboComponent, CqcKickPerformedEvent>(OnCQCKick);
        SubscribeLocalEvent<CanPerformComboComponent, CqcRestrainPerformedEvent>(OnCQCRestrain);
        SubscribeLocalEvent<CanPerformComboComponent, CqcPressurePerformedEvent>(OnCQCPressure);
        SubscribeLocalEvent<CanPerformComboComponent, CqcConsecutivePerformedEvent>(OnCQCConsecutive);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComboAttackPerformedEvent>(OnCQCAttackPerformed);
        SubscribeLocalEvent<GrantCqcComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCqcComponent, ExaminedEvent>(OnGrantCQCExamine);
    }

    #region Generic Methods

    private void OnGrantCQCUse(Entity<GrantCqcComponent> ent, ref UseInHandEvent args)
    {
        if (!_netManager.IsServer)
            return;

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

        _stamina.TakeStaminaDamage(args.Target, 15f);
    }

    #endregion

    #region Combo Methods

    private void OnCQCSlam(Entity<Components.CanPerformComboComponent> ent, ref CqcSlamPerformedEvent args)
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

    private void OnCQCKick(Entity<Components.CanPerformComboComponent> ent, ref CqcKickPerformedEvent args)
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

    private void OnCQCRestrain(Entity<Components.CanPerformComboComponent> ent, ref CqcRestrainPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out _))
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(10), true);
        _stamina.TakeStaminaDamage(target, 30f, source: ent);
    }

    private void OnCQCPressure(Entity<Components.CanPerformComboComponent> ent, ref CqcPressurePerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat, out var target, out _))
            return;

        _stamina.TakeStaminaDamage(target, 65f, source: ent);
        if (!_hands.TryGetActiveItem(target, out var activeItem))
            return;
        if(!_hands.TryDrop(target, activeItem.Value))
            return;
        if (!_hands.TryGetEmptyHand(target, out var emptyHand))
            return;
        if(!_hands.TryPickupAnyHand(ent, activeItem.Value))
            return;
        _hands.SetActiveHand(ent, emptyHand);
    }

    private void OnCQCConsecutive(Entity<Components.CanPerformComboComponent> ent,
        ref CqcConsecutivePerformedEvent args)
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
