using Content.Goobstation.Shared.ImmortalSnail;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.ImmortalSnail.Objectives;

/// <summary>
/// Handles Immortal Snail-specific objectives.
/// </summary>
public sealed class ImmortalSnailObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImmortalSnailComponent, ImmortalSnailTargetSetEvent>(OnTargetSet);
    }

    private void OnTargetSet(EntityUid uid, ImmortalSnailComponent comp, ref ImmortalSnailTargetSetEvent args)
    {
        // When the snail's target is set (when player accepts), update all objectives
        if (!_mind.TryGetMind(uid, out var _, out var snailMind))
            return;

        foreach (var objectiveUid in snailMind.Objectives)
            if (HasComp<KillPersonConditionComponent>(objectiveUid))
                UpdateObjectiveTarget(uid, comp, objectiveUid);
    }

    private void UpdateObjectiveTarget(EntityUid snailUid, ImmortalSnailComponent comp, EntityUid objectiveUid)
    {
        if (!TryComp<TargetObjectiveComponent>(objectiveUid, out var targetObjective)
            || comp.Target == null
            || !_mind.TryGetMind(comp.Target.Value, out var targetMindId, out _))
            return;

        _target.SetTarget(objectiveUid, targetMindId, targetObjective);
        _target.SetName(objectiveUid, targetObjective);
    }

    /// <summary>
    /// Adds objectives to the target player when they accept the snail offer.
    /// </summary>
    public void AddTargetObjectives(EntityUid targetMind)
    {
        if (!TryComp<MindComponent>(targetMind, out var mindComp))
            return;

        _mind.TryAddObjective(targetMind, mindComp, "ImmortalSnailSurviveObjective");
    }
}
