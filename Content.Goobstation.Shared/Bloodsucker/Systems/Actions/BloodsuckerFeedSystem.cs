using Content.Goobstation.Common.Grab;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Drains blood from a target.
/// </summary>
public sealed class BloodsuckerFeedSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

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

        var feeding = EnsureComp<BloodsuckerFeedingComponent>(ent);
        feeding.NetTarget = GetNetEntity(target);

        StartDoAfter(ent, target, comp.StartDelay);
    }

    private void OnFeedDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerFeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp(ent, out BloodsuckerFeedingComponent? feeding))
            return;

        var target = GetEntity(feeding.NetTarget);
        if (target == EntityUid.Invalid)
            return;

        if (!TryComp(ent, out BloodsuckerFeedComponent? comp))
            return;

        if (!InRange(ent.Owner, target))
        {
            RemCompDeferred<BloodsuckerFeedingComponent>(ent);
            return;
        }

        PerformDrain(ent, target, comp);

        // Debug blood levels just to see if its working. until I add UI.
        if (TryComp(target, out BloodstreamComponent? targetBlood)
            && TryComp(ent.Owner, out BloodstreamComponent? vampBlood)
            && targetBlood.BloodSolution is { } tSol
            && vampBlood.BloodSolution is { } vSol)
        {
            var msg = $"Target: {tSol.Comp.Solution.Volume}u | Vampire: {vSol.Comp.Solution.Volume}u";
            _popup.PopupPredicted(msg, ent.Owner, ent.Owner, PopupType.Small);
        }

        StartDoAfter(ent, target, 1f);
    }
    private void PerformDrain(Entity<BloodsuckerComponent> ent, EntityUid target, BloodsuckerFeedComponent comp)
    {
        float amount = comp.BloodDrainAmount;
        if (TryComp(ent, out BloodsuckerFrenzyComponent? frenzy))
            amount *= frenzy.FeedMultiplier;

        if (!TryComp(target, out BloodstreamComponent? targetBloodstream))
            return;

        if (!TryComp(ent.Owner, out BloodstreamComponent? vampireBloodstream))
            return;

        // Resolve the target's blood solution directly
        if (!_bloodstream.SolutionContainer.ResolveSolution(target, targetBloodstream.BloodSolutionName,
                ref targetBloodstream.BloodSolution, out var targetBloodSolution))
            return;

        var targetVolume = targetBloodSolution.Volume;
        var actualAmount = FixedPoint2.New(MathF.Min(amount, (float) targetVolume));
        if (actualAmount <= FixedPoint2.Zero)
            return;

        // delete blood from target
        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(target, targetBloodstream),
            -actualAmount);

        // Expand vampire max capacity if needed, then add blood
        if (_bloodstream.SolutionContainer.ResolveSolution(ent.Owner, vampireBloodstream.BloodSolutionName,
                ref vampireBloodstream.BloodSolution, out var vampBloodSolution))
        {
            var needed = vampBloodSolution.Volume + actualAmount;
            if (needed > vampBloodSolution.MaxVolume)
            {
                vampBloodSolution.MaxVolume = needed;
                _bloodstream.SolutionContainer.UpdateChemicals(vampireBloodstream.BloodSolution!.Value);
            }
        }

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(ent.Owner, vampireBloodstream),
            actualAmount);

        // Sleep if hard grabbed
        if (_pullerQuery.TryComp(ent.Owner, out var puller)
            && puller.Pulling == target
            && TryComp(target, out GrabbableComponent? grabbable)
            && grabbable.GrabStage >= GrabStage.Hard)
        {
            _status.TryAddStatusEffect(target, "ForcedSleep", out _, TimeSpan.FromSeconds(comp.SleepDuration));
        }

        _audio.PlayPredicted(comp.DrinkSound, ent, ent);
    }

    private void StartDoAfter(Entity<BloodsuckerComponent> ent, EntityUid target, float delay)
    {
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            delay,
            new BloodsuckerFeedDoAfterEvent(),
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
