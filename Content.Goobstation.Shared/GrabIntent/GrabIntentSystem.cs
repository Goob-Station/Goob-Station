using System.Numerics;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._White.Grab;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.GrabIntent;

public sealed class GrabIntentSystem : EntitySystem
{
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualSystem = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifierSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly HeldSpeedModifierSystem _clothingMoveSpeed = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;

    private readonly SoundPathSpecifier _thudswoosh = new("/Audio/Effects/thudswoosh.ogg");

    public override void Initialize()
    {
        SubscribeLocalEvent<GrabbableComponent, MoveInputEvent>(OnPullableMoveInput);
        SubscribeLocalEvent<GrabbableComponent, UpdateCanMoveEvent>(OnGrabbedMoveAttempt);
        SubscribeLocalEvent<GrabbableComponent, SpeakAttemptEvent>(OnGrabbedSpeakAttempt);
        SubscribeLocalEvent<GrabbableComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<GrabbableComponent, StoodEvent>(OnStood);
        SubscribeLocalEvent<GrabbableComponent, CheckGrabbedEvent>(OnCheckGrabbed);
        SubscribeLocalEvent<GrabbableComponent, GrabAttemptEvent>(OnGrabAttempt);
        SubscribeLocalEvent<GrabbableComponent, GrabAttemptReleaseEvent>(OnGrabReleaseAttempt);
        SubscribeLocalEvent<GrabbableComponent, PullStoppedMessage>(OnPullStoppedGrabbable);
        SubscribeLocalEvent<PullableComponent, PullStartedMessage>(OnPullStarted);

        SubscribeLocalEvent<GrabIntentComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<GrabIntentComponent, VirtualItemThrownEvent>(OnVirtualItemThrown);
        SubscribeLocalEvent<GrabIntentComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<GrabIntentComponent, PullStoppedMessage>(OnPullStoppedGrabIntent);
    }

    private void OnPullStarted(Entity<PullableComponent> ent, ref PullStartedMessage args)
    {
        if (args.PulledUid != ent.Owner || !CanGrab(args.PullerUid, ent.Owner))
            return;

        EnsureComp<GrabIntentComponent>(args.PullerUid);
        var grabbable = EnsureComp<GrabbableComponent>(args.PulledUid);
        _alertsSystem.ShowAlert(args.PulledUid, grabbable.PulledAlert, 0);
    }

    private void OnPullStoppedGrabbable(EntityUid uid, GrabbableComponent component, ref PullStoppedMessage args)
    {
        if (args.PulledUid != uid)
            return;

        component.GrabStage = GrabStage.No;
        component.GrabEscapeChance = 1f;
        component.EscapeAttemptModifier = 1f;
        _blocker.UpdateCanMove(uid);
        _alertsSystem.ClearAlert(uid, component.PulledAlert);
        Dirty(uid, component);
    }

    private void OnPullStoppedGrabIntent(EntityUid uid, GrabIntentComponent component, ref PullStoppedMessage args)
    {
        if (args.PullerUid != uid)
            return;

        component.GrabStage = GrabStage.No;
        foreach (var item in component.GrabVirtualItems)
        {
            if (TryComp<VirtualItemComponent>(item, out var virtualItemComponent))
                _virtualSystem.DeleteVirtualItem((item, virtualItemComponent), uid);
            else
                QueueDel(item);
        }

        component.GrabVirtualItems.Clear();
        Dirty(uid, component);
    }

    private void OnCheckGrabbed(EntityUid uid, GrabbableComponent component, ref CheckGrabbedEvent args)
    {
        args.IsGrabbed = component.GrabStage != GrabStage.No;
    }

    private void OnGrabReleaseAttempt(Entity<GrabbableComponent> ent, ref GrabAttemptReleaseEvent args)
    {
        args.Released = TryGrabRelease(ent.Owner, args.user, args.puller);
    }

    private void OnGrabAttempt(Entity<GrabbableComponent> ent, ref GrabAttemptEvent args)
    {
        if (!TryComp<PullableComponent>(ent, out var pullable))
            return;

        args.Grabbed = TryGrab((ent.Owner, pullable, ent.Comp), args.Puller, args.IgnoreCombatMode, args.GrabStageOverride, args.EscapeAttemptModifier);
    }

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

        var seedArray = new List<int>{(int) _timing.CurTick.Value, GetNetEntity(uid).Id};
        var seed = SharedRandomExtensions.HashCodeCombine(seedArray);
        var rand = new Random(seed);
        if (rand.Prob(grabbable.GrabEscapeChance))
            TryLowerGrabStage(args.User, uid, true);
    }

    private void OnRefreshMovespeed(EntityUid uid, GrabIntentComponent component, RefreshMovementSpeedModifiersEvent args)
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

    private void OnPullableMoveInput(EntityUid uid, GrabbableComponent component, ref MoveInputEvent args)
    {
        if (!TryComp<PullableComponent>(uid, out var pullable) || !pullable.BeingPulled)
            return;

        if (component.GrabStage == GrabStage.Soft)
            _pulling.TryStopPull(uid, pullable, uid);

        if (!_blocker.CanMove(args.Entity))
            return;

        _pulling.TryStopPull(uid, pullable, user: uid);
    }

    public bool CanGrab(EntityUid puller, EntityUid pullable)
    {
        return !HasComp<PacifiedComponent>(puller) && HasComp<MobStateComponent>(pullable);
    }

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
        _alertsSystem.ShowAlert(pullable.Owner, pullable.Comp2.PulledAlert, pullable.Comp2.PulledAlertAlertSeverity[stage]);
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

    /// <summary>
    /// Trying to grab the target
    /// </summary>
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

        return TryGrab((pullableUid, pullableComp, grabbableComp), pullerUid, ignoreCombatMode, grabStageOverride, escapeAttemptModifier);
    }

    /// <summary>
    /// Trying to grab the target
    /// </summary>
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

        // prevent you from grabbing someone else while being grabbed
        if (TryComp<PullableComponent>(pullerUid, out var pullerAsPullable) && pullerAsPullable.Puller != null)
            return false;
        // Don't grab without grab intent
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

        // It's blocking stage update, maybe better UX?
        if (grabIntentComp.GrabStage == GrabStage.Suffocate)
        {
            _stamina.TakeStaminaDamage(pullable.Owner, grabIntentComp.SuffocateGrabStaminaDamage, applyResistances: true);

            var comboEv =
                new ComboAttackPerformedEvent(pullerUid, pullable.Owner, pullerUid, ComboAttackType.Grab);
            RaiseLocalEvent(pullerUid, comboEv);
            _audio.PlayPredicted(_thudswoosh, pullable.Owner, pullerUid);
            return true;
        }

        // Update stage
        var nextStageAddition = grabIntentComp.GrabStageDirection switch
        {
            GrabStageDirection.Increase => 1,
            GrabStageDirection.Decrease => -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var newStage = grabIntentComp.GrabStage + nextStageAddition;

        if (HasComp<MartialArtsKnowledgeComponent>(pullerUid) // i really hate this solution holy fuck
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

    private bool TryUpdateGrabVirtualItems(
        Entity<PullerComponent, GrabIntentComponent> puller,
        Entity<PullableComponent, GrabbableComponent> pullable)
    {
        var grabItemEv = new FindGrabbingItemEvent(pullable.Owner);
        RaiseLocalEvent(puller.Owner, ref grabItemEv);
        if (grabItemEv.GrabbingItem != null)
            return true;

        var virtualItemsCount = puller.Comp2.GrabVirtualItems.Count;

        var newVirtualItemsCount = puller.Comp1.NeedsHands ? 0 : 1;
        if (puller.Comp2.GrabVirtualItemStageCount.TryGetValue(puller.Comp2.GrabStage, out var count))
            newVirtualItemsCount += count;

        if (virtualItemsCount == newVirtualItemsCount)
            return true;
        var delta = newVirtualItemsCount - virtualItemsCount;

        if (delta > 0)
        {
            for (var i = 0; i < delta; i++)
            {
                var emptyHand = _handsSystem.TryGetEmptyHand(puller.Owner, out _);
                if (!emptyHand || !_virtualSystem.TrySpawnVirtualItemInHand(pullable.Owner, puller.Owner, out var item, true))
                {
                    _popup.PopupPredicted(Loc.GetString("popup-grab-need-hand"), puller.Owner, puller.Owner, PopupType.Medium);
                    return false;
                }

                puller.Comp2.GrabVirtualItems.Add(item.Value);
            }
        }

        if (delta >= 0)
            return true;

        for (var i = 0; i < Math.Abs(delta); i++)
        {
            if (i >= puller.Comp2.GrabVirtualItems.Count)
                break;

            var item = puller.Comp2.GrabVirtualItems[i];
            puller.Comp2.GrabVirtualItems.Remove(item);
            if (TryComp<VirtualItemComponent>(item, out var virtualItemComponent))
                _virtualSystem.DeleteVirtualItem((item, virtualItemComponent), puller.Owner);
        }

        return true;
    }

    /// <summary>
    /// Attempts to release entity from grab
    /// </summary>
    private GrabResistResult GrabRelease(Entity<GrabbableComponent?> pullable)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp, false))
            return GrabResistResult.Succeeded;

        if (_timing.CurTime < pullable.Comp.NextEscapeAttempt)
            return GrabResistResult.TooSoon;

        // TODO: MOVE THIS AND THE ONE BELOW TO PREDICTED RANDOM WHEN ENGINE PR MERGED
        var seedArray = new List<int>{(int) _timing.CurTick.Value, GetNetEntity(pullable.Owner).Id};
        var seed = SharedRandomExtensions.HashCodeCombine(seedArray);
        var rand = new Random(seed);
        if (rand.Prob(pullable.Comp.GrabEscapeChance))
            return GrabResistResult.Succeeded;
        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(pullable.Comp.EscapeAttemptCooldown));
        Dirty(pullable.Owner, pullable.Comp);
        return GrabResistResult.Failed;
    }

    private bool TryGrabRelease(EntityUid pullableUid, EntityUid? user, EntityUid pullerUid)
    {
        if (user == null || user.Value != pullableUid)
            return true;

        if (!TryComp<GrabbableComponent>(pullableUid, out var grabbable))
            return true;

        var releaseAttempt = GrabRelease((pullableUid, grabbable));
        switch (releaseAttempt)
        {
            case GrabResistResult.Failed:
                _popup.PopupPredicted(Loc.GetString("popup-grab-release-fail-self"),
                    pullableUid,
                    pullableUid,
                    PopupType.SmallCaution);
                return false;
            case GrabResistResult.TooSoon:
                _popup.PopupPredicted(Loc.GetString("popup-grab-release-too-soon"),
                    pullableUid,
                    pullableUid,
                    PopupType.SmallCaution);
                return false;
        }

        _popup.PopupPredicted(Loc.GetString("popup-grab-release-success-self"),
            pullableUid,
            pullableUid,
            PopupType.SmallCaution);
        _popup.PopupPredicted(
            Loc.GetString("popup-grab-release-success-puller",
                ("target", Identity.Entity(pullableUid, EntityManager))),
            pullerUid,
            pullerUid,
            PopupType.MediumCaution);
        return true;
    }

    private void OnGrabbedMoveAttempt(EntityUid uid, GrabbableComponent component, UpdateCanMoveEvent args)
    {
        if (component.GrabStage == GrabStage.No)
            return;

        args.Cancel();
    }

    private void OnGrabbedSpeakAttempt(EntityUid uid, GrabbableComponent component, SpeakAttemptEvent args)
    {
        if (component.GrabStage != GrabStage.Suffocate)
            return;

        _popup.PopupPredicted(Loc.GetString("popup-grabbed-cant-speak"), uid, uid, PopupType.MediumCaution);
        args.Cancel();
    }

    /// <summary>
    /// Tries to lower grab stage for target or release it
    /// </summary>
    public bool TryLowerGrabStage(EntityUid pullableUid, EntityUid pullerUid, bool ignoreCombatMode = false)
    {
        if (!TryComp<PullableComponent>(pullableUid, out var pullableComp)
            || !TryComp<GrabbableComponent>(pullableUid, out var grabbableComp)
            || !TryComp<PullerComponent>(pullerUid, out var pullerComp)
            || !TryComp<GrabIntentComponent>(pullerUid, out var grabIntentComp))
            return false;

        return TryLowerGrabStage((pullableUid, pullableComp, grabbableComp), (pullerUid, pullerComp, grabIntentComp), ignoreCombatMode);
    }

    /// <summary>
    /// Tries to lower grab stage for target or release it
    /// </summary>
    public bool TryLowerGrabStage(
        Entity<PullableComponent?, GrabbableComponent?> pullable,
        Entity<PullerComponent?, GrabIntentComponent?> puller,
        bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp1, ref pullable.Comp2) || !Resolve(puller.Owner, ref puller.Comp1, ref puller.Comp2))
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
        TrySetGrabStages((puller.Owner, puller.Comp1, puller.Comp2), (pullable.Owner, pullable.Comp1, pullable.Comp2), newStage);
        return true;
    }

    private void OnVirtualItemThrown(EntityUid uid, GrabIntentComponent component, ref VirtualItemThrownEvent args)
    {
        if (!TryComp<PullerComponent>(uid, out var puller)
            || puller.Pulling == null
            || puller.Pulling != args.BlockingEntity)
            return;

        ThrowGrabbedEntity(uid, args.Direction);
    }

    public void ThrowGrabbedEntity(Entity<PullerComponent?, GrabIntentComponent?, PhysicsComponent?> ent, Vector2 dir)
    {
        if (!Resolve(ent.Owner, ref ent.Comp1, ref ent.Comp2, ref ent.Comp3, false)
            || ent.Comp1.Pulling is not { } pulling
            || !TryComp(pulling, out PullableComponent? pullingPullableComp))
            return;

        if (!_combatMode.IsInCombatMode(ent.Owner)
            || HasComp<GrabThrownComponent>(pulling)
            || ent.Comp2.GrabStage <= GrabStage.Soft)
            return;

        var distanceToCursor = dir.Length();
        var direction = dir.Normalized() * MathF.Min(distanceToCursor, ent.Comp2.ThrowingDistance);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 5);

        _pulling.TryStopPull(pulling, pullingPullableComp, ent.Owner, true);
        _grabThrown.Throw(pulling,
            ent.Owner,
            direction,
            ent.Comp2.GrabThrownSpeed,
            damage * ent.Comp2.GrabThrowDamageModifier);
        _throwing.TryThrow(ent.Owner, -direction * ent.Comp3.InvMass);
        _audio.PlayPredicted(_thudswoosh, pulling, ent.Owner);
        ent.Comp2.NextStageChange = _timing.CurTime.Add(TimeSpan.FromSeconds(3f));
        Dirty(ent.Owner, ent.Comp2);
    }
}
