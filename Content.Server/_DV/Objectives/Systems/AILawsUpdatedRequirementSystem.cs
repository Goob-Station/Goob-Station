using Content.Server._DV.Objectives.Components;
using Content.Server._DV.Objectives.Events;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
///     Handles the AI laws updated objective.
/// </summary>
public sealed class AILawsUpdatedRequirementSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _code = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AILawsUpdatedRequirementComponent, RequirementCheckEvent>(OnRequirementCheck);
        SubscribeLocalEvent<AILawUpdatedEvent>(OnLawInserted);
    }

    private void OnRequirementCheck(Entity<AILawsUpdatedRequirementComponent> entity, ref RequirementCheckEvent args)
    {
        // Ensure there is an AI on the station.
        var allMinds = EntityQueryEnumerator<MindComponent>();
        var found = false;
        while (allMinds.MoveNext(out var mind, out _))
        {
            if (_job.MindHasJobWithId(mind, "StationAi"))
                found = true;
        }

        args.Cancelled = !found;
    }

    private void OnLawInserted(AILawUpdatedEvent args)
    {
        // We only want the station AI
        if (!_mind.TryGetMind(args.Target, out var mindUid, out _) || ! _job.MindHasJobWithId(mindUid, "StationAi"))
            return;

        var query = EntityQueryEnumerator<AILawsUpdatedRequirementComponent>();

        while (query.MoveNext(out var uid, out var aiLawsObj))
        {
            if (aiLawsObj.Lawset == args.Lawset)
                _code.SetCompleted(uid);
        }
    }
}
