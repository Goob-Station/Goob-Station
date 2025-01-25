using Content.Server.Objectives.Components;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;
using System.Linq;
using Content.Server._Goobstation.Objectives.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles keep alive condition logic and picking random traitors to keep alive.
/// </summary>
public sealed class KeepAliveConditionSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entity = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KeepAliveConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<RandomTraitorAliveComponent, ObjectiveAssignedEvent>(OnAssigned);

        SubscribeLocalEvent<RandomTraitorTargetComponent, ObjectiveAssignedEvent>(OnTraitorTargetAssigned);
    }

    private void OnGetProgress(EntityUid uid, KeepAliveConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(uid, out var target))
            return;

        args.Progress = GetProgress(target.Value);
    }

    private void OnAssigned(EntityUid uid, RandomTraitorAliveComponent comp, ref ObjectiveAssignedEvent args)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
        {
            args.Cancelled = true;
            return;
        }

        var traitors = Enumerable.ToList<(EntityUid Id, MindComponent Mind)>(_traitorRule.GetOtherTraitorMindsAliveAndConnected(args.Mind));

        // You are the first/only traitor.
        if (traitors.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(uid, _random.Pick(traitors).Id, target);
    }

    // Goobstation - Protect another traitor's target
    private void OnTraitorTargetAssigned(EntityUid uid,
        RandomTraitorTargetComponent comp,
        ref ObjectiveAssignedEvent args)
    {
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
        {
            args.Cancelled = true;
            return;
        }

        var tots = _traitorRule.GetOtherTraitorMindsAliveAndConnected(args.Mind);
        var killTargets = new List<EntityUid>();

        foreach (var tot in tots)
        {
            var objectives = tot.Mind.Objectives;
            foreach (var obj in objectives)
            {
                // Check for kill cond
                if (!_entity.HasComponent<KillPersonConditionComponent>(obj))
                    continue;

                // Check if target is present
                if (!_entity.TryGetComponent<TargetObjectiveComponent>(obj, out var targComp))
                    continue;

                if (targComp.Target == null)
                    continue;

                // Check if target is us
                if (targComp.Target == args.MindId)
                    continue;

                killTargets.Add(targComp.Target.Value);
            }
        }

        // No other traitors or no traitors with valid kill objectives
        if (killTargets.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(uid, _random.Pick(killTargets), target);
    }

    private float GetProgress(EntityUid target)
    {
        if (!TryComp<MindComponent>(target, out var mind))
            return 0f;

        return _mind.IsCharacterDeadIc(mind) ? 0f : 1f;
    }
}
