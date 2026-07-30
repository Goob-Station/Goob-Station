// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Movement.Events;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

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

    private EntityQuery<BloodstreamComponent> _bloodstreamQuery;
    private EntityQuery<HungerComponent> _hungerQuery;
    private EntityQuery<SlimeComponent> _slimeQuery;
    private EntityQuery<XenoVacuumTankComponent> _tankQuery;

    private TimeSpan _updateDelay = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, SlimeLatchEvent>(OnLatchAttempt);
        SubscribeLocalEvent<SlimeComponent, SlimeLatchDoAfterEvent>(OnSlimeLatchDoAfter);

        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnMobStateChangedSOD);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnMobStateChangedSlime);
        SubscribeLocalEvent<SlimeComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<SlimeComponent, EntGotRemovedFromContainerMessage>(OnEntGotRemovedFromContainer);
        SubscribeLocalEvent<SlimeComponent, EntGotInsertedIntoContainerMessage>(OnEntGotInsertedIntoContainer);
        SubscribeLocalEvent<SlimeComponent, SlimeMitosisEvent>(OnSlimeMitosis);
        SubscribeLocalEvent<SlimeComponent, UpdateCanMoveEvent>(OnUpdateCanMove);

        _bloodstreamQuery = GetEntityQuery<BloodstreamComponent>();
        _hungerQuery = GetEntityQuery<HungerComponent>();
        _slimeQuery = GetEntityQuery<SlimeComponent>();
        _tankQuery = GetEntityQuery<XenoVacuumTankComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _gameTiming.CurTime;

        if (now < _nextUpdate)
            return;

        _nextUpdate = now + _updateDelay;

        var query = EntityQueryEnumerator<SlimeDamageOvertimeComponent>();
        while (query.MoveNext(out var uid, out var dotComp))
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
            && _solutionContainer.ResolveSolution(ent.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var blood)
            && _solutionContainer.ResolveSolution(ent.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var chem))
        {
            var totalVolume = chem.Volume + blood.Volume;
            if (totalVolume <= 0)
                return;

            var bloodProportion = blood.Volume / totalVolume;
            var chemProportion = 1 - bloodProportion;
            var bloodTransfer = FixedPoint2.Min(ent.Comp.SuctionUnits * bloodProportion, availableVolume * bloodProportion);
            var chemTransfer = FixedPoint2.Min(ent.Comp.SuctionUnits * chemProportion, availableVolume * chemProportion);

            foreach (var stomach in stomachList)
            {
                var bloodSolution = blood.SplitSolutionWithout(bloodTransfer / FixedPoint2.New(stomachList.Count), ent.Comp.ToxinReagent); // we don't want slime sucking it's own toxin instad of drinking blood
                _stomach.TryTransferSolution(stomach.Owner, bloodSolution, stomach); // blood first, other chemicals later

                var chemSolution = blood.SplitSolution(chemTransfer / FixedPoint2.New(stomachList.Count));
                _stomach.TryTransferSolution(stomach.Owner, chemSolution, stomach);
            }

            //chem.AddReagent(ent.Comp.ToxinReagent, ent.Comp.ToxinUnits);
        }
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

    private void OnUpdateCanMove(Entity<SlimeComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (IsLatched(ent))
            args.Cancel();
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

    private void OnSlimeMitosis(Entity<SlimeComponent> ent, ref SlimeMitosisEvent args)
    {
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
        };

        EnsureComp<BeingLatchedComponent>(target);

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return false;

        return true;
    }

    private void OnSlimeLatchDoAfter(Entity<SlimeComponent> ent, ref SlimeLatchDoAfterEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (args.Handled || args.Cancelled)
        {
            RemCompDeferred<BeingLatchedComponent>(target);
            return;
        }

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
            || !HasComp<MobStateComponent>(target)); // make any mob work
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

        EnsureComp(target, out SlimeDamageOvertimeComponent comp);
        comp.SourceEntityUid = ent;
        Dirty(target, comp);

        _audio.PlayPredicted(ent.Comp.EatSound, ent, ent);
        _popup.PopupPredicted(Loc.GetString("slime-action-latch-success", ("slime", ent), ("target", target)), ent, ent, PopupType.Small);

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
        ent.Comp.LatchedTarget = null;
        _actionBlocker.UpdateCanMove(ent.Owner);
    }

    #endregion
}
