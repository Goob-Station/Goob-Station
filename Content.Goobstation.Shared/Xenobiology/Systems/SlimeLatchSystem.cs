// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Sleeping;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared._Goobstation.Sleep;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.ActionBlocker;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Climbing.Events;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology;

// This handles any actions that slime mobs may have.
public sealed partial class SlimeLatchSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly SharedPhysicsSystem _physic = default!;

    private EntityQuery<BloodstreamComponent> _bloodstreamQuery;
    private EntityQuery<HungerComponent> _hungerQuery;
    private EntityQuery<SlimeComponent> _slimeQuery;
    private EntityQuery<XenoVacuumTankComponent> _tankQuery;
    private EntityQuery<MobStateComponent> _mobQuery;
    private EntityQuery<BeingLatchedComponent> _latchedQuery;


    private TimeSpan _updateDelay = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdate;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, SlimeLatchEvent>(OnLatchAttempt);
        SubscribeLocalEvent<SlimeComponent, SlimeLatchDoAfterEvent>(OnSlimeLatchDoAfter);
        SubscribeLocalEvent<SlimeComponent, DoAfterAttemptEvent<SlimeLatchDoAfterEvent>>(OnDoAfterAttempt);

        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnMobStateChangedSOD);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnMobStateChangedSlime);
        SubscribeLocalEvent<SlimeComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<SlimeComponent, EntGotRemovedFromContainerMessage>(OnEntGotRemovedFromContainer);
        SubscribeLocalEvent<SlimeComponent, EntGotInsertedIntoContainerMessage>(OnEntGotInsertedIntoContainer);
        SubscribeLocalEvent<SlimeComponent, SelfBeforeClimbEvent>(OnSelfBeforeClimb);
        SubscribeLocalEvent<SlimeComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<SlimeDamageOvertimeComponent, WakeDamageOverrideEvent>(OnWakeOverride);
        SubscribeLocalEvent<SlimeDamageOvertimeComponent, SleepOverrideEvent>(OnSleepOverride);

        _bloodstreamQuery = GetEntityQuery<BloodstreamComponent>();
        _hungerQuery = GetEntityQuery<HungerComponent>();
        _slimeQuery = GetEntityQuery<SlimeComponent>();
        _tankQuery = GetEntityQuery<XenoVacuumTankComponent>();
        _mobQuery = GetEntityQuery<MobStateComponent>();
        _latchedQuery = GetEntityQuery<BeingLatchedComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _gameTiming.CurTime;

        if (now < _nextUpdate)
            return;

        _nextUpdate = now + _updateDelay;

        var query = EntityQueryEnumerator<SlimeDamageOvertimeComponent, BodyComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var dotComp, out var _, out var _))
        {
            if (_mobState.IsDead(uid))
                continue;

            UpdateHunger((uid, dotComp));
        }
    }

    private void UpdateHunger(Entity<SlimeDamageOvertimeComponent> ent)
    {
        var addedHunger = (float) ent.Comp.Damage.GetTotal();

        _damageable.TryChangeDamage(ent, ent.Comp.Damage, ignoreResistances: true, targetPart: TargetBodyPart.All);

        if (ent.Comp.SourceEntityUid is not { } source)
            return;

        if (_hungerQuery.TryComp(source, out var hunger))
            _hunger.ModifyHunger(source, addedHunger, hunger);

        var stomachList = _body.GetBodyOrganEntityComps<StomachComponent>(source);

        if (stomachList.Count == 0)
            return;

        FixedPoint2 availableVolume = 0;
        foreach (var stomach in stomachList)
        {
            if (_solutionContainer.ResolveSolution(stomach.Owner, StomachSystem.DefaultSolutionName, ref stomach.Comp1.Solution, out var sol))
                availableVolume += sol.AvailableVolume;
        }

        if (_bloodstreamQuery.TryComp(ent, out var bloodstream)
            && _solutionContainer.ResolveSolution(ent.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var blood))
        {
            var chem = blood; // Don't resolve twice

            var totalVolume = chem.Volume + blood.Volume;

            if (totalVolume <= 0)
                return;

            var bloodProportion = blood.Volume / totalVolume;
            var chemProportion = 1 - bloodProportion;
            var bloodTransfer = FixedPoint2.Min(ent.Comp.SuctionUnits * bloodProportion, availableVolume * bloodProportion);
            var chemTransfer = FixedPoint2.Min(ent.Comp.SuctionUnits * chemProportion, availableVolume * chemProportion);

            var stomachCount = FixedPoint2.New(stomachList.Count);
            foreach (var stomach in stomachList)
            {
                var bloodSolution = blood.SplitSolutionWithout(bloodTransfer / stomachCount, ent.Comp.ToxinReagent); // we don't want slime sucking it's own toxin instad of drinking blood
                _stomach.TryTransferSolution(stomach.Owner, bloodSolution, stomach); // blood first, other chemicals later

                var chemSolution = chem.SplitSolution(chemTransfer / stomachCount);
                _stomach.TryTransferSolution(stomach.Owner, chemSolution, stomach);
            }

            chem.AddReagent(ent.Comp.ToxinReagent, ent.Comp.ToxinUnits);
        }
    }

    private void OnWakeOverride(Entity<SlimeDamageOvertimeComponent> ent, ref WakeDamageOverrideEvent args)
    {
        args.IgnoreDamage = true;
    }

    private void OnSleepOverride(Entity<SlimeDamageOvertimeComponent> ent, ref SleepOverrideEvent args)
    {
        if (!TryComp<MobStateComponent>(ent.Owner, out var mobState))
            return;

        args.MobState = mobState.CurrentState;
    }

    private void OnMobStateChangedSOD(Entity<SlimeDamageOvertimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var source = ent.Comp.SourceEntityUid;
        if (source.HasValue && _slimeQuery.TryComp(source, out var slime))
            Unlatch((source.Value, slime));
    }

    private void OnMobStateChangedSlime(Entity<SlimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            Unlatch(ent);
    }

    private void OnPullAttempt(Entity<SlimeComponent> ent, ref PullAttemptEvent args)
    {
        if (IsLatched(ent) && args.PullerUid == ent.Owner) // slimes can't pull when latched
        {
            args.Cancelled = true;
            return;
        }

        Unlatch(ent);
    }

    private void OnEntGotRemovedFromContainer(Entity<SlimeComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!_tankQuery.HasComp(args.Container.Owner))
            return;

        Unlatch(ent);
    }

    private void OnEntGotInsertedIntoContainer(Entity<SlimeComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!_tankQuery.HasComp(args.Container.Owner))
            return;

        Unlatch(ent);
    }

    private void OnLatchAttempt(Entity<SlimeComponent> ent, ref SlimeLatchEvent args)
    {
        if (TerminatingOrDeleted(args.Target)
        || TerminatingOrDeleted(ent.Owner))
            return;

        if (IsLatched(ent))
        {
            Unlatch(ent);
            return;
        }

        if (CanLatch(ent, args.Target))
        {
            StartSlimeLatchDoAfter(ent, args.Target);
            return;
        }

        // improvement space (tm)
    }

    private void OnUpdateCanMove(Entity<SlimeComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (IsLatched(ent))
            args.Cancel();
    }

    private bool StartSlimeLatchDoAfter(Entity<SlimeComponent> ent, EntityUid target)
    {
        if (_mobState.IsDead(target))
        {
            var targetDeadPopup = Loc.GetString("slime-latch-fail-target-dead", ("ent", target));
            _popup.PopupPredicted(targetDeadPopup, ent, ent);

            return false;
        }

        if (ent.Comp.Stomach.Count >= ent.Comp.MaxContainedEntities)
        {
            var maxEntitiesPopup = Loc.GetString("slime-latch-fail-max-entities", ("ent", target));
            _popup.PopupPredicted(maxEntitiesPopup, ent, ent);

            return false;
        }

        var attemptPopup = Loc.GetString("slime-latch-attempt", ("slime", ent), ("ent", target));
        _popup.PopupPredicted(attemptPopup, ent, ent, PopupType.MediumCaution);

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.LatchDoAfterDuration, new SlimeLatchDoAfterEvent(), ent, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return false;

        return true;
    }

    private void OnSelfBeforeClimb(Entity<SlimeComponent> ent, ref SelfBeforeClimbEvent args)
    {
        if (IsLatched(ent))
            Unlatch(ent); // Unlatch first so no accident dot
    }

    private void OnDoAfterAttempt(EntityUid uid, SlimeComponent comp, ref DoAfterAttemptEvent<SlimeLatchDoAfterEvent> args)
    {
        if (HasComp<BeingLatchedComponent>(args.Event.Target))
            args.Cancel();
    }

    private void OnSlimeLatchDoAfter(Entity<SlimeComponent> ent, ref SlimeLatchDoAfterEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (args.Handled || args.Cancelled)
            return;

        Latch(ent, target);
        args.Handled = true;
    }

    #region Helpers

    public bool IsLatched(Entity<SlimeComponent> ent)
        => ent.Comp.LatchedTarget.HasValue;

    public bool IsLatched(Entity<SlimeComponent> ent, EntityUid target)
        => IsLatched(ent) && ent.Comp.LatchedTarget!.Value == target;

    public bool CanLatch(Entity<SlimeComponent> ent, EntityUid target)
    {
        return !(IsLatched(ent) // already latched
            || _mobState.IsDead(target) // target dead
            || !_actionBlocker.CanInteract(ent, target) // can't reach
            || !_mobQuery.HasComp(target) // any mob
            || _latchedQuery.HasComp(target)); // already claimed
    }

    public bool NpcTryLatch(Entity<SlimeComponent> ent, EntityUid target)
    {
        if (!CanLatch(ent, target))
            return false;

        return StartSlimeLatchDoAfter(ent, target);
    }

    public void Latch(Entity<SlimeComponent> ent, EntityUid target)
    {
        if (IsLatched(ent))
            Unlatch(ent);

        _xform.SetCoordinates(ent, Transform(target).Coordinates);
        _xform.SetParent(ent, target);

        ent.Comp.LatchedTarget = target;
        Dirty(ent);
        _actionBlocker.UpdateCanMove(ent.Owner);

        EnsureComp<BeingLatchedComponent>(target);
        EnsureComp(target, out SlimeDamageOvertimeComponent comp);
        comp.SourceEntityUid = ent;

        var physic = EnsureComp<PhysicsComponent>(ent.Owner);
        var fixture = EnsureComp<FixturesComponent>(ent.Owner);
        _physic.SetCanCollide(ent.Owner, false, force: true, manager: fixture, body: physic); // For some reaosn the slime will collide with host and moving them


        _audio.PlayEntity(ent.Comp.EatSound, ent, ent);
        _popup.PopupPredicted(Loc.GetString("slime-action-latch-success", ("slime", ent), ("target", target)), ent, ent);

        Dirty(ent);
        Dirty(target, comp);

        // We also need to set a new state for the slime when it's consuming,
        // this will be easy however it's important to take MobGrowthSystem into account... possibly we should use layers?
    }

    public void Unlatch(Entity<SlimeComponent> ent)
    {
        if (!IsLatched(ent))
            return;

        var target = ent.Comp.LatchedTarget!.Value;

        RemCompDeferred<BeingLatchedComponent>(target);
        RemCompDeferred<SlimeDamageOvertimeComponent>(target);

        _xform.SetParent(ent, _xform.GetParentUid(target)); // deparent it. probably.

        var physic = EnsureComp<PhysicsComponent>(ent.Owner);
        var fixture = EnsureComp<FixturesComponent>(ent.Owner);
        _physic.SetCanCollide(ent.Owner, true, force: true, manager: fixture, body: physic); // Make the slime collide back


        ent.Comp.LatchedTarget = null;
        _actionBlocker.UpdateCanMove(ent.Owner);
        Dirty(ent);
    }

    #endregion
}
