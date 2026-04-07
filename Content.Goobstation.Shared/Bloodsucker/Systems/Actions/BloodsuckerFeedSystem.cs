using Content.Goobstation.Common.Grab;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared._White.Grab;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.GameObjects;
using System;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Drains blood from a target.
/// </summary>
public sealed class BloodsuckerFeedSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;

    private EntityQuery<PullerComponent> _pullerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _pullerQuery = GetEntityQuery<PullerComponent>();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerFeedEvent>(OnFeedStart);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerFeedDoAfterEvent>(OnFeedDoAfter);
    }

    private void OnFeedStart(Entity<BloodsuckerComponent> ent, ref BloodsuckerFeedEvent args)
    {
        if (!TryComp(ent, out BloodsuckerFeedComponent? comp))
            return;

        var target = args.Target;

        if (target == EntityUid.Invalid || target == ent.Owner)
            return;

        if (!InRange(ent.Owner, target))
            return;

        if (!TryUseCosts(ent, comp))
            return;

        StartDoAfter(ent, target, comp.StartDelay);
    }

    private void OnFeedDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerFeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var target = EnsureEntity<BloodsuckerFeedingComponent>(args.NetTarget, ent.Owner);

        if (!TryComp(ent, out BloodsuckerFeedComponent? comp))
            return;

        if (!InRange(ent.Owner, target))
        {
            RemCompDeferred<BloodsuckerFeedingComponent>(ent);
            return;
        }

        // Keep feeding state alive
        var feeding = EnsureComp<BloodsuckerFeedingComponent>(ent);
        feeding.NetTarget = GetNetEntity(target);

        PerformDrain(ent, target, comp);

        // Queue the next tick
        StartDoAfter(ent, target, 1f);
    }

    private void PerformDrain(Entity<BloodsuckerComponent> ent, EntityUid target, BloodsuckerFeedComponent comp)
    {
        // Frenzy multiplier
        float amount = comp.BloodDrainAmount;
        if (TryComp(ent, out BloodsuckerFrenzyComponent? frenzy))
            amount *= frenzy.FeedMultiplier;

        if (!TryComp(target, out BloodstreamComponent? targetBloodstream))
            return;

        if (!_solution.TryGetSolution(target, targetBloodstream.BloodSolutionName, out _, out var targetBloodSolution))
            return;

        // Clamp to what the target actually has
        var actualAmount = MathF.Min(amount, (float) targetBloodSolution.Volume);
        if (actualAmount <= 0f)
            return;

        // They looze da blood
        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(target, targetBloodstream),
            FixedPoint2.New(-actualAmount));

        // I soock the bloood
        if (TryComp(ent.Owner, out BloodstreamComponent? vampireBloodstream))
            _bloodstream.TryModifyBloodLevel(
                new Entity<BloodstreamComponent?>(ent.Owner, vampireBloodstream),
                FixedPoint2.New(actualAmount));

        // Sleep if hardgrab
        if (_pullerQuery.TryComp(ent.Owner, out var puller)
            && puller.Pulling == target
            && TryComp(target, out GrabbableComponent? grabbable)
            && grabbable.GrabStage >= GrabStage.Hard)
        {
            _status.TryAddStatusEffect(target, "ForcedSleep", out _, TimeSpan.FromSeconds(comp.SleepDuration));
        }
    }

    private void StartDoAfter(Entity<BloodsuckerComponent> ent, EntityUid target, float delay)
    {
        var netTarget = GetNetEntity(target);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            delay,
            new BloodsuckerFeedDoAfterEvent(netTarget),
            ent.Owner,
            target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerFeedComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        // Deduct blood cost from the vampire's own bloodstream
        if (comp.BloodCost > 0f)
        {
            if (!TryComp(ent.Owner, out BloodstreamComponent? bloodstream))
                return false;

            var currentVolume = bloodstream.BloodSolution is { } sol
                ? (float) sol.Comp.Solution.Volume
                : 0f;
            if (currentVolume < comp.BloodCost)
                return false;

            _bloodstream.TryModifyBloodLevel(
                new Entity<BloodstreamComponent?>(ent.Owner, bloodstream),
                FixedPoint2.New(-comp.BloodCost));
        }

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity), -comp.HumanityCost);

        return true;
    }

    private bool InRange(EntityUid a, EntityUid b)
    {
        if (!TryComp(a, out TransformComponent? ta) ||
            !TryComp(b, out TransformComponent? tb))
            return false;

        if (ta.MapID != tb.MapID)
            return false;

        var delta = ta.WorldPosition - tb.WorldPosition;
        return MathF.Abs(delta.X) <= 1.5f && MathF.Abs(delta.Y) <= 1.5f;
    }
}
