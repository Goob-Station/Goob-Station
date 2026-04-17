// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
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
using Content.Shared.Chemistry.Components;

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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeLatchEvent>(OnLatchAttempt);
        SubscribeLocalEvent<SlimeComponent, SlimeLatchDoAfterEvent>(OnSlimeLatchDoAfter);
        SubscribeLocalEvent<SlimeComponent, DoAfterAttemptEvent<SlimeLatchDoAfterEvent>>(OnDoAfterAttempt);

        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnMobStateChangedSOD);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnMobStateChangedSlime);
        SubscribeLocalEvent<SlimeComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<SlimeComponent, EntGotRemovedFromContainerMessage>(OnEntGotRemovedFromContainer);
        SubscribeLocalEvent<SlimeComponent, EntGotInsertedIntoContainerMessage>(OnEntGotInsertedIntoContainer);
        SubscribeLocalEvent<SlimeComponent, SlimeMitosisEvent>(OnSlimeMitosis);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var sodQuery = EntityQueryEnumerator<SlimeDamageOvertimeComponent>();
        while (sodQuery.MoveNext(out var uid, out var dotComp))
            UpdateHunger((uid, dotComp));
    }

    private void UpdateHunger(Entity<SlimeDamageOvertimeComponent> ent)
    {
        if (_gameTiming.CurTime < ent.Comp.NextTickTime || _mobState.IsDead(ent))
            return;

        ent.Comp.NextTickTime = _gameTiming.CurTime + ent.Comp.Interval;
        _damageable.TryChangeDamage(ent, ent.Comp.Damage, ignoreResistances: true, targetPart: TargetBodyPart.All);

        if (ent.Comp.SourceEntityUid is not { } source)
            return;

        var addedHunger = (float) ent.Comp.Damage.GetTotal();
        if (TryComp<HungerComponent>(source, out var hunger))
        {
            _hunger.ModifyHunger(source, addedHunger, hunger);
            Dirty(source, hunger);
        }

        var stomachList = _body.GetBodyOrganEntityComps<StomachComponent>(source);

        if (stomachList.Count == 0)
            return;

        FixedPoint2 availabaleVolume = 0;
        foreach (var stomach in stomachList)
        {
            if (_solutionContainer.ResolveSolution(stomach.Owner, StomachSystem.DefaultSolutionName, ref stomach.Comp1.Solution, out var sol))
                availabaleVolume += sol.AvailableVolume;
        }

        if (TryComp<BloodstreamComponent>(ent, out var bloodstream)
            && _solutionContainer.ResolveSolution(ent.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var blood)
            && _solutionContainer.ResolveSolution(ent.Owner, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chem))
        {
            FixedPoint2 bloodProportion = blood.Volume/(chem.Volume + blood.Volume);
            FixedPoint2 chemProportion = 1 - bloodProportion;
            FixedPoint2 bloodTransfer = FixedPoint2.Min(ent.Comp.SuctionUnits * bloodProportion, availabaleVolume * bloodProportion);
            FixedPoint2 chemTransfer = FixedPoint2.Min(ent.Comp.SuctionUnits * chemProportion, availabaleVolume * chemProportion);
            foreach (var stomach in stomachList)
            {
                var bloodSolution = blood.SplitSolutionWithout(bloodTransfer/FixedPoint2.New(stomachList.Count), ent.Comp.ToxinReagent); // we don't want slime sucking it's own toxin instad of drinking blood
                _stomach.TryTransferSolution(stomach.Owner, bloodSolution, stomach); // blood first, other chemicals later
                var chemSolution = blood.SplitSolution(chemTransfer/FixedPoint2.New(stomachList.Count));
                _stomach.TryTransferSolution(stomach.Owner, chemSolution, stomach);
            }
            chem.AddReagent(ent.Comp.ToxinReagent, ent.Comp.ToxinUnits);
        }
    }

    private void OnMobStateChangedSOD(Entity<SlimeDamageOvertimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var source = ent.Comp.SourceEntityUid;
        if (source.HasValue && TryComp<SlimeComponent>(source, out var slime))
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
        Unlatch(ent);
    }

    private void OnEntGotInsertedIntoContainer(Entity<SlimeComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        Unlatch(ent);
    }

    private void OnSlimeMitosis(Entity<SlimeComponent> ent, ref SlimeMitosisEvent args)
    {
        Unlatch(ent);
    }

    private void OnLatchAttempt(SlimeLatchEvent args)
    {
        if (TerminatingOrDeleted(args.Target)
        || TerminatingOrDeleted(args.Performer)
        || !TryComp<SlimeComponent>(args.Performer, out var slime))
            return;

        var ent = new Entity<SlimeComponent>(args.Performer, slime);

        if (IsLatched(ent))
        {
            Unlatch(ent);
            return;
        }

        if (CanLatch((args.Performer, slime), args.Target))
        {
            StartSlimeLatchDoAfter((args.Performer, slime), args.Target);
            return;
        }

        // improvement space (tm)
    }

    private bool StartSlimeLatchDoAfter(Entity<SlimeComponent> ent, EntityUid target)
    {
        if (_mobState.IsDead(target))
        {
            var targetDeadPopup = Loc.GetString("slime-latch-fail-target-dead", ("ent", target));
            _popup.PopupEntity(targetDeadPopup, ent, ent);

            return false;
        }

        if (ent.Comp.Stomach.Count >= ent.Comp.MaxContainedEntities)
        {
            var maxEntitiesPopup = Loc.GetString("slime-latch-fail-max-entities", ("ent", target));
            _popup.PopupEntity(maxEntitiesPopup, ent, ent);

            return false;
        }

        if (HasComp<BeingLatchedComponent>(target))
        {
            var maxEntitiesPopup = Loc.GetString("slime-latch-fail-already-latched", ("ent", target));
            _popup.PopupEntity(maxEntitiesPopup, ent, ent);

            return false;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.LatchDoAfterDuration, new SlimeLatchDoAfterEvent(), ent, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return false;

        var attemptPopup = Loc.GetString("slime-latch-attempt", ("slime", ent), ("ent", target));
        _popup.PopupEntity(attemptPopup, ent, PopupType.MediumCaution);
        return true;
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
        if (TryComp<InputMoverComponent>(ent, out var inpm))
            inpm.CanMove = false;

        ent.Comp.LatchedTarget = target;

        EnsureComp<BeingLatchedComponent>(target);
        EnsureComp(target, out SlimeDamageOvertimeComponent comp);
        comp.SourceEntityUid = ent;

        _audio.PlayEntity(ent.Comp.EatSound, ent, ent);
        _popup.PopupEntity(Loc.GetString("slime-action-latch-success", ("slime", ent), ("target", target)), ent, PopupType.SmallCaution);

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

        if (TryComp<TransformComponent>(target, out var targetXform)
            && _xform.IsParentOf(targetXform, ent.Owner))
            _xform.SetParent(ent.Owner, _xform.GetParentUid(target));

        if (TryComp<InputMoverComponent>(ent, out var inpm))
            inpm.CanMove = true;

        ent.Comp.LatchedTarget = null;
    }

    #endregion
}
