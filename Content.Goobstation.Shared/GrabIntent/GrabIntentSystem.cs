using System.Numerics;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._White.Grab;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Cuffs;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory.VirtualItem;
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
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;

    // TODO: make a GrabIntent component move this there
    // TODO: move any grab related data into said component so we aint bloating up  puller/pullable
    private readonly SoundPathSpecifier _thudswoosh = new("/Audio/Effects/thudswoosh.ogg");

    public override void Initialize()
    {
        SubscribeLocalEvent<PullableComponent, UpdateCanMoveEvent>(OnGrabbedMoveAttempt);
        SubscribeLocalEvent<PullableComponent, SpeakAttemptEvent>(OnGrabbedSpeakAttempt);
        SubscribeLocalEvent<PullableComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<PullableComponent, StoodEvent>(OnStood);
        SubscribeLocalEvent<PullableComponent, GrabAttemptEvent>(OnGrabAttempt);
        SubscribeLocalEvent<PullableComponent, GrabAttemptReleaseEvent>(OnGrabReleaseAttempt);
        SubscribeLocalEvent<PullerComponent, AddCuffDoAfterEvent>(OnAddCuffDoAfterEvent);
        SubscribeLocalEvent<PullerComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<PullerComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<PullerComponent, VirtualItemThrownEvent>(OnVirtualItemThrown);
    }

    private void OnGrabReleaseAttempt(Entity<PullableComponent> ent, ref GrabAttemptReleaseEvent args)
    {
        args.Released = TryGrabRelease(ent, args.user, args.puller);
    }

    private void OnGrabAttempt(Entity<PullableComponent> ent, ref GrabAttemptEvent args)
    {
        args.Grabbed = TryGrab(ent.Owner, args.Puller, args.IgnoreCombatMode, args.GrabStageOverride, args.EscapeAttemptModifier);
    }

    private void OnDowned(Entity<PullableComponent> ent, ref DownedEvent args)
    {
        if (!TryComp(ent.Comp.Puller, out PullerComponent? puller))
            return;

        ResetGrabEscapeChance(ent, (ent.Comp.Puller.Value, puller));
    }

    private void OnStood(Entity<PullableComponent> ent, ref StoodEvent args)
    {
        if (!TryComp(ent.Comp.Puller, out PullerComponent? puller))
            return;

        ResetGrabEscapeChance(ent, (ent.Comp.Puller.Value, puller));
    }

    private void OnAttacked(Entity<PullerComponent> ent, ref AttackedEvent args)
    {
        if (ent.Comp.Pulling != args.User
            || ent.Comp.GrabStage < GrabStage.Soft
            || !TryComp(args.User, out PullableComponent? pullable))
            return;

        var seed = SharedRandomExtensions.HashCodeCombine([(int) _timing.CurTick.Value, GetNetEntity(ent).Id]);
        var rand = new Random(seed);
        if (rand.Prob(pullable.GrabEscapeChance))
            TryLowerGrabStage((args.User, pullable), (ent.Owner, ent.Comp), true);
    }

    private void OnAddCuffDoAfterEvent(Entity<PullerComponent> ent, ref AddCuffDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || !TryComp<PullableComponent>(ent.Comp.Pulling, out var comp)
            || ent.Comp.Pulling == null)
            return;

        _pulling.TryStopPull(ent.Comp.Pulling.Value, comp);
    }
    public bool CanGrab(EntityUid puller, EntityUid pullable)
    {
        return !HasComp<PacifiedComponent>(puller) && HasComp<MobStateComponent>(pullable);
    }

    public bool TrySetGrabStages(Entity<PullerComponent> puller, Entity<PullableComponent> pullable, GrabStage stage, float escapeAttemptModifier = 1f)
    {
        puller.Comp.GrabStage = stage;
        pullable.Comp.GrabStage = stage;
        pullable.Comp.EscapeAttemptModifier *= escapeAttemptModifier;
        if (!TryUpdateGrabVirtualItems(puller, pullable))
            return false;

        var popupType = GetPopupType(stage);
        ResetGrabEscapeChance(pullable, puller, false);
        _alertsSystem.ShowAlert(puller, puller.Comp.PullingAlert, puller.Comp.PullingAlertSeverity[stage]);
        _alertsSystem.ShowAlert(pullable, pullable.Comp.PulledAlert, pullable.Comp.PulledAlertAlertSeverity[stage]);
        _blocker.UpdateCanMove(pullable);
        _modifierSystem.RefreshMovementSpeedModifiers(puller);
        GrabStagePopup(puller, pullable, popupType);

        var comboEv = new ComboAttackPerformedEvent(puller.Owner, pullable.Owner, puller.Owner, ComboAttackType.Grab);
        RaiseLocalEvent(puller.Owner, comboEv);

        Dirty(pullable);
        Dirty(puller);
        return true;
    }

    // TODO: This should probably just be a dictionary we index instead of a whole method.
    // TODO: or better yet protos..
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

    private void GrabStagePopup(Entity<PullerComponent> puller, Entity<PullableComponent> pullable, PopupType popupType)
    {
        var grabStageString = puller.Comp.GrabStage.ToString().ToLower();
        _popup.PopupPredicted(Loc.GetString($"popup-grab-{grabStageString}-self",
                ("target", Identity.Entity(pullable, EntityManager))),
            Loc.GetString($"popup-grab-{grabStageString}-others",
                ("target", Identity.Entity(pullable, EntityManager)),
                ("puller", Identity.Entity(puller, EntityManager))),
            pullable,
            puller,
            PopupType.Medium);
        _popup.PopupPredicted(
            Loc.GetString($"popup-grab-{grabStageString}-target",
                ("puller", Identity.Entity(puller, EntityManager))),
                null,
            pullable,
            pullable,
            popupType);
        _audio.PlayPredicted(_thudswoosh, pullable, null);
    }

    /// <summary>
    /// Trying to grab the target
    /// </summary>
    /// <param name="pullable">Target that would be grabbed</param>
    /// <param name="puller">Performer of the grab</param>
    /// <param name="ignoreCombatMode">If true, will ignore disabled combat mode</param>
    /// <param name="grabStageOverride">What stage to set the grab too from the start</param>
    /// <param name="escapeAttemptModifier">if anything what to modify the escape chance by</param>
    /// <returns>If grab was successful</returns>
    public bool TryGrab(Entity<PullableComponent?> pullable,
        Entity<PullerComponent?> puller,
        bool ignoreCombatMode = false,
        GrabStage? grabStageOverride = null,
        float escapeAttemptModifier = 1f)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp)
            || !Resolve(puller.Owner, ref puller.Comp)
            || !CanGrab(puller, pullable)
            || pullable.Comp.Puller != puller
            || puller.Comp.Pulling != pullable
            || !TryComp<MeleeWeaponComponent>(puller, out var meleeWeaponComponent))
            return false;

        // prevent you from grabbing someone else while being grabbed
        if (TryComp<PullableComponent>(puller, out var pullerAsPullable) && pullerAsPullable.Puller != null)
            return false;

        // Don't grab without grab intent
        if (!ignoreCombatMode && !_combatMode.IsInCombatMode(puller))
            return false;

        if (_timing.CurTime < meleeWeaponComponent.NextAttack)
            return false;

        var max = meleeWeaponComponent.NextAttack > _timing.CurTime ? meleeWeaponComponent.NextAttack : _timing.CurTime;
        var attackRateEv = new GetMeleeAttackRateEvent(puller, meleeWeaponComponent.AttackRate, 1, puller);
        RaiseLocalEvent(puller, ref attackRateEv);
        meleeWeaponComponent.NextAttack = puller.Comp.StageChangeCooldown * attackRateEv.Multipliers + max;
        Dirty(puller, meleeWeaponComponent);

        var beforeEvent = new BeforeHarmfulActionEvent(puller, HarmfulActionType.Grab);
        RaiseLocalEvent(pullable, beforeEvent);
        if (beforeEvent.Cancelled)
            return false;

        // It's blocking stage update, maybe better UX?
        if (puller.Comp.GrabStage == GrabStage.Suffocate)
        {
            _stamina.TakeStaminaDamage(pullable, puller.Comp.SuffocateGrabStaminaDamage, applyResistances: true);

            var comboEv =
                new ComboAttackPerformedEvent(puller.Owner, pullable.Owner, puller.Owner, ComboAttackType.Grab);
            RaiseLocalEvent(puller.Owner, comboEv);
            _audio.PlayPredicted(_thudswoosh, pullable.Owner, puller.Owner);
            Dirty(pullable);
            Dirty(puller);
            return true;
        }

        // Update stage
        // TODO: Change grab stage direction
        var nextStageAddition = puller.Comp.GrabStageDirection switch
        {
            GrabStageDirection.Increase => 1,
            GrabStageDirection.Decrease => -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var newStage = puller.Comp.GrabStage + nextStageAddition;

        if (HasComp<MartialArtsKnowledgeComponent>(puller) // i really hate this solution holy fuck
            && TryComp<RequireProjectileTargetComponent>(pullable, out var layingDown)
            && layingDown.Active)
        {
            var ev = new CheckGrabOverridesEvent(newStage);
            RaiseLocalEvent(puller, ev);
            newStage = ev.Stage;
        }

        if (grabStageOverride != null)
            newStage = grabStageOverride.Value;

        var raiseEv = new RaiseGrabModifierEventEvent(puller.Owner, (int) newStage);
        RaiseLocalEvent(ref raiseEv);
        if (raiseEv.NewStage != null)
            newStage = (GrabStage) raiseEv.NewStage;

        if (!TrySetGrabStages((puller, puller.Comp), (pullable, pullable.Comp), newStage, escapeAttemptModifier))
            return false;

        var raiseEffectList = new List<EntityUid> { pullable };
        _color.RaiseEffect(Color.Yellow,
            raiseEffectList,
            Filter.Pvs(pullable, entityManager: EntityManager));
        return true;
    }
    private void ResetGrabEscapeChance(
        Entity<PullableComponent> pullable,
        Entity<PullerComponent> puller,
        bool dirty = true)
    {
        if (puller.Comp.GrabStage == GrabStage.No)
        {
            pullable.Comp.GrabEscapeChance = 1f;
            if (dirty)
                Dirty(pullable);
            return;
        }

        var massMultiplier = Math.Clamp(_contests.MassContest(pullable, puller, true) * 2f, 0.5f, 2f);
        var extraMultiplier = 1f;
        if (_standing.IsDown(pullable))
            extraMultiplier *= puller.Comp.DownedEscapeChanceMultiplier;
        var raiseEv = new RaiseGrabModifierEventEvent(puller.Owner, 0);
        RaiseLocalEvent(ref raiseEv);
        extraMultiplier *= raiseEv.Multiplier;

        var chance = puller.Comp.EscapeChances[puller.Comp.GrabStage] * massMultiplier *
            pullable.Comp.EscapeAttemptModifier * extraMultiplier + raiseEv.Modifier;
        pullable.Comp.GrabEscapeChance = Math.Clamp(chance, 0f, 1f);

        if (dirty)
            Dirty(pullable);
    }

    private bool TryUpdateGrabVirtualItems(Entity<PullerComponent> puller, Entity<PullableComponent> pullable)
    {
        var grabItemEv = new FindGrabbingItemEvent(pullable);
        RaiseLocalEvent(puller, ref grabItemEv);
        if (grabItemEv.GrabbingItem != null)
            return true;

        // Updating virtual items
        var virtualItemsCount = puller.Comp.GrabVirtualItems.Count;

        var newVirtualItemsCount = puller.Comp.NeedsHands ? 0 : 1;
        if (puller.Comp.GrabVirtualItemStageCount.TryGetValue(puller.Comp.GrabStage, out var count))
            newVirtualItemsCount += count;

        if (virtualItemsCount == newVirtualItemsCount)
            return true;
        var delta = newVirtualItemsCount - virtualItemsCount;

        // Adding new virtual items
        if (delta > 0)
        {
            for (var i = 0; i < delta; i++)
            {
                var emptyHand = _handsSystem.TryGetEmptyHand(puller.Owner, out _);
                if (!emptyHand || !_virtualSystem.TrySpawnVirtualItemInHand(pullable, puller.Owner, out var item, true))
                {
                    _popup.PopupPredicted(Loc.GetString("popup-grab-need-hand"), puller, puller, PopupType.Medium);

                    return false;
                }
                puller.Comp.GrabVirtualItems.Add(item.Value);
            }
        }

        if (delta >= 0)
            return true;
        for (var i = 0; i < Math.Abs(delta); i++)
        {
            if (i >= puller.Comp.GrabVirtualItems.Count)
                break;

            var item = puller.Comp.GrabVirtualItems[i];
            puller.Comp.GrabVirtualItems.Remove(item);
            if(TryComp<VirtualItemComponent>(item, out var virtualItemComponent))
                _virtualSystem.DeleteVirtualItem((item,virtualItemComponent), puller);
        }

        return true;
    }

    /// <summary>
    /// Attempts to release entity from grab
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <returns></returns>
    private GrabResistResult GrabRelease(Entity<PullableComponent?> pullable)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp)
            || _timing.CurTime < pullable.Comp.NextEscapeAttempt)
            return GrabResistResult.TooSoon;
        var seed = SharedRandomExtensions.HashCodeCombine([(int) _timing.CurTick.Value, GetNetEntity(pullable).Id]);
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
        var releaseAttempt = GrabRelease(pullableUid);
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

    private void OnGrabbedMoveAttempt(EntityUid uid, PullableComponent component, UpdateCanMoveEvent args)
    {
        if (!_timing.ApplyingState)
            return;
        if (component.GrabStage == GrabStage.No)
            return;
        args.Cancel();
    }

    private void OnGrabbedSpeakAttempt(EntityUid uid, PullableComponent component, SpeakAttemptEvent args)
    {
        if (component.GrabStage != GrabStage.Suffocate)
            return;

        _popup.PopupPredicted(Loc.GetString("popup-grabbed-cant-speak"), uid, uid, PopupType.MediumCaution);   // You cant speak while someone is choking you

        args.Cancel();
    }

    /// <summary>
    /// Tries to lower grab stage for target or release it
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <param name="puller">Performer</param>
    /// <param name="ignoreCombatMode">If true, will NOT release target if combat mode is off</param>
    /// <returns></returns>
    public bool TryLowerGrabStage(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp) || !Resolve(puller.Owner, ref puller.Comp))
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1f));
        Dirty(pullable);
        Dirty(puller);

        if (!ignoreCombatMode && _combatMode.IsInCombatMode(puller.Owner) || puller.Comp.GrabStage == GrabStage.No)
        {
            _pulling.TryStopPull(pullable, pullable.Comp, ignoreGrab: true);
            return true;
        }

        var newStage = puller.Comp.GrabStage - 1;
        TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage);
        return true;
    }

    private void OnVirtualItemThrown(Entity<PullerComponent> ent, ref VirtualItemThrownEvent args)
    {
        if (ent.Comp.Pulling == null || ent.Comp.Pulling != args.BlockingEntity)
            return;
        ThrowGrabbedEntity(ent.Owner, args.Direction);
    }

    public void ThrowGrabbedEntity(Entity<PullerComponent?, PhysicsComponent?> ent, Vector2 dir)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, false)
            || !TryComp(ent.Comp1.Pulling, out PullableComponent? pullingPullableComp))
            return;
        var pulling = ent.Comp1.Pulling.Value;

        if (!_combatMode.IsInCombatMode(ent)
            || HasComp<GrabThrownComponent>(pulling)
            || ent.Comp1.GrabStage <= GrabStage.Soft)
            return;

        var distanceToCursor = dir.Length();
        var direction = dir.Normalized() * MathF.Min(distanceToCursor, ent.Comp1.ThrowingDistance);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 5);

        _pulling.TryStopPull(pulling, pullingPullableComp, ent, true);
        _grabThrown.Throw(pulling,
            ent,
            direction,
            ent.Comp1.GrabThrownSpeed,
            damage * ent.Comp1.GrabThrowDamageModifier);
        _throwing.TryThrow(ent, -direction * ent.Comp2.InvMass);
        _audio.PlayPredicted(_thudswoosh, pulling, ent);
        ent.Comp1.NextStageChange = _timing.CurTime.Add(TimeSpan.FromSeconds(3f));
        Dirty(ent, ent.Comp1);
    }

    private void OnVirtualItemDeleted(Entity<PullerComponent> ent, ref VirtualItemDeletedEvent args)
    {
        // If client deletes the virtual hand then stop the pull.
        if (ent.Comp.Pulling == null || ent.Comp.Pulling != args.BlockingEntity)
            return;

        if (TryComp(args.BlockingEntity, out PullableComponent? comp))
            _pulling.TryStopPull(ent.Comp.Pulling.Value, comp, ent);

        foreach (var item in ent.Comp.GrabVirtualItems)
        {
            if (TryComp<VirtualItemComponent>(item, out var virtualItemComponent))
                _virtualSystem.DeleteVirtualItem((item, virtualItemComponent), ent);
        }

        ent.Comp.GrabVirtualItems.Clear();
    }
}
