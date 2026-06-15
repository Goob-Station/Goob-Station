using System.Linq;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._White.Grab;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.Damage.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Standing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.GrabIntent;

public sealed partial class GrabIntentSystem
{
    #region Stage Initialization

    private void InitializeGrabStageEvents()
    {
        SubscribeLocalEvent<GrabbableComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<GrabbableComponent, StoodEvent>(OnStood);

        SubscribeLocalEvent<GrabIntentComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<GrabIntentComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
    }

    #endregion

    #region Stage Events

    private void OnDowned(Entity<GrabbableComponent> ent, ref DownedEvent args)
    {
        if (!TryComp<PullableComponent>(ent, out var pullable)
            || pullable.Puller is not { } pullerUid
            || !TryComp<PullerComponent>(pullerUid, out var puller)
            || !TryComp<GrabIntentComponent>(pullerUid, out var grabIntent))
            return;

        ResetGrabEscapeChance((ent.Owner, pullable, ent.Comp), (pullerUid, puller, grabIntent));
    }

    private void OnStood(Entity<GrabbableComponent> ent, ref StoodEvent args)
    {
        if (!TryComp<PullableComponent>(ent, out var pullable)
            || pullable.Puller is not { } pullerUid
            || !TryComp<PullerComponent>(pullerUid, out var puller)
            || !TryComp<GrabIntentComponent>(pullerUid, out var grabIntent))
            return;

        ResetGrabEscapeChance((ent.Owner, pullable, ent.Comp), (pullerUid, puller, grabIntent));
    }

    private void OnAttacked(EntityUid uid, GrabIntentComponent component, ref AttackedEvent args)
    {
        if (!TryComp<PullerComponent>(uid, out var puller)
            || puller.Pulling != args.User
            || component.GrabStage < GrabStage.Soft
            || !TryComp<GrabbableComponent>(args.User, out var grabbable))
            return;

        var seedArray = new List<int> { (int) _timing.CurTick.Value, GetNetEntity(uid).Id };
        var seed = SharedRandomExtensions.HashCodeCombine(seedArray);
        var rand = new Random(seed);
        if (rand.Prob(grabbable.GrabEscapeChance))
            TryLowerGrabStage(args.User, uid, true);
    }

    private void OnRefreshMovespeed(EntityUid uid,
        GrabIntentComponent component,
        RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<PullerComponent>(uid, out var puller))
            return;

        if (TryComp<HeldSpeedModifierComponent>(puller.Pulling, out var itemHeldSpeed))
        {
            var (walkMod, sprintMod) =
                _clothingMoveSpeed.GetHeldMovementSpeedModifiers(puller.Pulling.Value, itemHeldSpeed);
            args.ModifySpeed(walkMod, sprintMod);
        }

        var raiseEv = new RaiseGrabModifierEventEvent(uid, 0);
        RaiseLocalEvent(ref raiseEv);
        var multiplier = raiseEv.SpeedMultiplier;
        var max = 1f;

        switch (component.GrabStage)
        {
            case GrabStage.Soft:
                max = MathF.Max(max, component.SoftGrabSpeedModifier);
                multiplier *= component.SoftGrabSpeedModifier;
                break;
            case GrabStage.Hard:
                max = MathF.Max(max, component.HardGrabSpeedModifier);
                multiplier *= component.HardGrabSpeedModifier;
                break;
            case GrabStage.Suffocate:
                max = MathF.Max(max, component.ChokeGrabSpeedModifier);
                multiplier *= component.ChokeGrabSpeedModifier;
                break;
        }

        multiplier = Math.Clamp(multiplier, 0f, max);
        args.ModifySpeed(multiplier, multiplier);
    }

    #endregion

    #region Stage Logic

    public bool TrySetGrabStages(
        Entity<PullerComponent, GrabIntentComponent> puller,
        Entity<PullableComponent, GrabbableComponent> pullable,
        GrabStage stage,
        float escapeAttemptModifier = 1f)
    {
        puller.Comp2.GrabStage = stage;
        pullable.Comp2.GrabStage = stage;
        pullable.Comp2.EscapeAttemptModifier *= escapeAttemptModifier;
        if (!TryUpdateGrabVirtualItems(puller, pullable))
            return false;

        var popupType = GetPopupType(stage);
        ResetGrabEscapeChance(pullable, puller, false);
        _alertsSystem.ShowAlert(puller.Owner, puller.Comp1.PullingAlert, puller.Comp2.PullingAlertSeverity[stage]);
        _alertsSystem.ShowAlert(pullable.Owner,
            pullable.Comp2.PulledAlert,
            pullable.Comp2.PulledAlertAlertSeverity[stage]);
        _blocker.UpdateCanMove(pullable.Owner);
        _modifierSystem.RefreshMovementSpeedModifiers(puller.Owner);
        GrabStagePopup(puller, pullable, popupType);

        var comboEv = new ComboAttackPerformedEvent(puller.Owner, pullable.Owner, puller.Owner, ComboAttackType.Grab);
        RaiseLocalEvent(puller.Owner, comboEv);

        Dirty(pullable.Owner, pullable.Comp2);
        Dirty(puller.Owner, puller.Comp2);
        return true;
    }

    private static PopupType GetPopupType(GrabStage stage)
    {
        var popupType = stage switch
        {
            GrabStage.No or GrabStage.Soft => PopupType.Small,
            GrabStage.Hard => PopupType.MediumCaution,
            GrabStage.Suffocate => PopupType.LargeCaution,
            _ => throw new ArgumentOutOfRangeException(),
        };
        return popupType;
    }

    private void GrabStagePopup(
        Entity<PullerComponent, GrabIntentComponent> puller,
        Entity<PullableComponent, GrabbableComponent> pullable,
        PopupType popupType)
    {
        var grabStageString = puller.Comp2.GrabStage.ToString().ToLower();
        _popup.PopupPredicted(Loc.GetString($"popup-grab-{grabStageString}-self",
                ("target", Identity.Entity(pullable.Owner, EntityManager))),
            Loc.GetString($"popup-grab-{grabStageString}-others",
                ("target", Identity.Entity(pullable.Owner, EntityManager)),
                ("puller", Identity.Entity(puller.Owner, EntityManager))),
            pullable.Owner,
            puller.Owner,
            PopupType.Medium);
        _popup.PopupPredicted(
            Loc.GetString($"popup-grab-{grabStageString}-target",
                ("puller", Identity.Entity(puller.Owner, EntityManager))),
            null,
            pullable.Owner,
            pullable.Owner,
            popupType);
        _audio.PlayPredicted(_thudswoosh, pullable.Owner, null);
    }

    public bool TryGrab(
        EntityUid pullableUid,
        EntityUid pullerUid,
        bool ignoreCombatMode = false,
        GrabStage? grabStageOverride = null,
        float escapeAttemptModifier = 1f)
    {
        if (!TryComp<PullableComponent>(pullableUid, out var pullableComp)
            || !TryComp<GrabbableComponent>(pullableUid, out var grabbableComp))
            return false;

        return TryGrab((pullableUid, pullableComp, grabbableComp),
            pullerUid,
            ignoreCombatMode,
            grabStageOverride,
            escapeAttemptModifier);
    }

    public bool TryGrab(
        Entity<PullableComponent?, GrabbableComponent?> pullable,
        EntityUid pullerUid,
        bool ignoreCombatMode = false,
        GrabStage? grabStageOverride = null,
        float escapeAttemptModifier = 1f)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp1, ref pullable.Comp2)
            || !TryComp<PullerComponent>(pullerUid, out var pullerComp)
            || !TryComp<GrabIntentComponent>(pullerUid, out var grabIntentComp)
            || !CanGrab(pullerUid, pullable.Owner)
            || pullable.Comp1.Puller != pullerUid
            || pullerComp.Pulling != pullable.Owner
            || !TryComp<MeleeWeaponComponent>(pullerUid, out var meleeWeaponComponent))
            return false;

        if (TryComp<PullableComponent>(pullerUid, out var pullerAsPullable) && pullerAsPullable.Puller != null)
            return false;

        if (!ignoreCombatMode && !_combatMode.IsInCombatMode(pullerUid))
            return false;

        if (_timing.CurTime < meleeWeaponComponent.NextAttack)
            return true;

        var max = meleeWeaponComponent.NextAttack > _timing.CurTime ? meleeWeaponComponent.NextAttack : _timing.CurTime;
        var attackRateEv = new GetMeleeAttackRateEvent(pullerUid, meleeWeaponComponent.AttackRate, 1, pullerUid);
        RaiseLocalEvent(pullerUid, ref attackRateEv);
        meleeWeaponComponent.NextAttack = grabIntentComp.StageChangeCooldown * attackRateEv.Multipliers + max;
        Dirty(pullerUid, meleeWeaponComponent);

        var beforeEvent = new BeforeHarmfulActionEvent(pullerUid, HarmfulActionType.Grab);
        RaiseLocalEvent(pullable.Owner, beforeEvent);
        if (beforeEvent.Cancelled)
            return false;

        if (grabIntentComp.GrabStage == GrabStage.Suffocate)
        {
            _stamina.TakeStaminaDamage(pullable.Owner,
                grabIntentComp.SuffocateGrabStaminaDamage,
                applyResistances: true);

            var comboEv =
                new ComboAttackPerformedEvent(pullerUid, pullable.Owner, pullerUid, ComboAttackType.Grab);
            RaiseLocalEvent(pullerUid, comboEv);
            _audio.PlayPredicted(_thudswoosh, pullable.Owner, pullerUid);
            return true;
        }

        var nextStageAddition = grabIntentComp.GrabStageDirection switch
        {
            GrabStageDirection.Increase => 1,
            GrabStageDirection.Decrease => -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var newStage = grabIntentComp.GrabStage + nextStageAddition;

        if (HasComp<MartialArtsKnowledgeComponent>(pullerUid)
            && TryComp<RequireProjectileTargetComponent>(pullable.Owner, out var layingDown)
            && layingDown.Active)
        {
            var ev = new CheckGrabOverridesEvent(newStage);
            RaiseLocalEvent(pullerUid, ev);
            newStage = ev.Stage;
        }

        if (grabStageOverride != null)
            newStage = grabStageOverride.Value;

        var raiseEv = new RaiseGrabModifierEventEvent(pullerUid, (int) newStage);
        RaiseLocalEvent(ref raiseEv);
        if (raiseEv.NewStage != null)
            newStage = (GrabStage) raiseEv.NewStage;

        var resolvedPuller = (pullerUid, pullerComp, grabIntentComp);
        var resolvedPullable = (pullable.Owner, pullable.Comp1, pullable.Comp2);
        if (!TrySetGrabStages(resolvedPuller, resolvedPullable, newStage, escapeAttemptModifier))
            return false;

        var raiseEffectList = new List<EntityUid> { pullable.Owner };
        _color.RaiseEffect(Color.Yellow,
            raiseEffectList,
            Filter.Pvs(pullable.Owner, entityManager: EntityManager));
        return true;
    }

    private void ResetGrabEscapeChance(
        Entity<PullableComponent, GrabbableComponent> pullable,
        Entity<PullerComponent, GrabIntentComponent> puller,
        bool dirty = true)
    {
        if (puller.Comp2.GrabStage == GrabStage.No)
        {
            pullable.Comp2.GrabEscapeChance = 1f;
            if (dirty)
                Dirty(pullable.Owner, pullable.Comp2);
            return;
        }

        var massMultiplier = Math.Clamp(_contests.MassContest(pullable.Owner, puller.Owner, true) * 2f, 0.5f, 2f);
        var extraMultiplier = 1f;
        if (_standing.IsDown(pullable.Owner))
            extraMultiplier *= puller.Comp2.DownedEscapeChanceMultiplier;
        var raiseEv = new RaiseGrabModifierEventEvent(puller.Owner, 0);
        RaiseLocalEvent(ref raiseEv);
        extraMultiplier *= raiseEv.Multiplier;

        var chance = puller.Comp2.EscapeChances[puller.Comp2.GrabStage] * massMultiplier *
            pullable.Comp2.EscapeAttemptModifier * extraMultiplier + raiseEv.Modifier;
        pullable.Comp2.GrabEscapeChance = Math.Clamp(chance, 0f, 1f);

        if (dirty)
            Dirty(pullable.Owner, pullable.Comp2);
    }

    private List<EntityUid> GetGrabVirtualItems(EntityUid puller, EntityUid pullable)
    {
        return _handsSystem.EnumerateHeld(puller)
            .Where(held => TryComp<VirtualItemComponent>(held, out var vi) && vi.BlockingEntity == pullable)
            .ToList();
    }

    private bool TryUpdateGrabVirtualItems(
        Entity<PullerComponent, GrabIntentComponent> puller,
        Entity<PullableComponent, GrabbableComponent> pullable)
    {
        var grabItemEv = new FindGrabbingItemEvent(pullable.Owner);
        RaiseLocalEvent(puller.Owner, ref grabItemEv);
        if (grabItemEv.GrabbingItem != null)
            return true;

        var targetCount = puller.Comp1.NeedsHands ? 0 : 1;
        if (puller.Comp2.GrabVirtualItemStageCount.TryGetValue(puller.Comp2.GrabStage, out var stageCount))
            targetCount += stageCount;

        var pullBaseline = puller.Comp1.NeedsHands ? 1 : 0;
        var grabManaged = GetGrabVirtualItems(puller.Owner, pullable.Owner).Skip(pullBaseline).ToList();
        var delta = targetCount - grabManaged.Count;

        if (delta > 0)
        {
            for (var i = 0; i < delta; i++)
            {
                if (_handsSystem.TryGetEmptyHand(puller.Owner, out _)
                    && _virtualSystem.TrySpawnVirtualItemInHand(pullable.Owner, puller.Owner, out _, true))
                    continue;
                _popup.PopupPredicted(Loc.GetString("popup-grab-need-hand"), puller.Owner, puller.Owner, PopupType.Medium);
                return false;
            }
        }
        else if (delta < 0)
        {
            foreach (var item in grabManaged.Take(-delta))
            {
                if (TryComp<VirtualItemComponent>(item, out var vi))
                    _virtualSystem.DeleteVirtualItem((item, vi), puller.Owner);
            }
        }

        return true;
    }

    public bool TryLowerGrabStage(EntityUid pullableUid, EntityUid pullerUid, bool ignoreCombatMode = false)
    {
        if (!TryComp<PullableComponent>(pullableUid, out var pullableComp)
            || !TryComp<GrabbableComponent>(pullableUid, out var grabbableComp)
            || !TryComp<PullerComponent>(pullerUid, out var pullerComp)
            || !TryComp<GrabIntentComponent>(pullerUid, out var grabIntentComp))
            return false;

        return TryLowerGrabStage((pullableUid, pullableComp, grabbableComp),
            (pullerUid, pullerComp, grabIntentComp),
            ignoreCombatMode);
    }

    public bool TryLowerGrabStage(
        Entity<PullableComponent?, GrabbableComponent?> pullable,
        Entity<PullerComponent?, GrabIntentComponent?> puller,
        bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp1, ref pullable.Comp2) ||
            !Resolve(puller.Owner, ref puller.Comp1, ref puller.Comp2))
            return false;

        if (pullable.Comp1.Puller != puller.Owner ||
            puller.Comp1.Pulling != pullable.Owner)
            return false;

        pullable.Comp2.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1f));
        Dirty(pullable.Owner, pullable.Comp2);
        Dirty(puller.Owner, puller.Comp2);

        if (!ignoreCombatMode && _combatMode.IsInCombatMode(puller.Owner) || puller.Comp2.GrabStage == GrabStage.No)
        {
            _pulling.TryStopPull(pullable.Owner, pullable.Comp1, ignoreGrab: true);
            return true;
        }

        var newStage = puller.Comp2.GrabStage - 1;
        TrySetGrabStages((puller.Owner, puller.Comp1, puller.Comp2),
            (pullable.Owner, pullable.Comp1, pullable.Comp2),
            newStage);
        return true;
    }

    #endregion
}
