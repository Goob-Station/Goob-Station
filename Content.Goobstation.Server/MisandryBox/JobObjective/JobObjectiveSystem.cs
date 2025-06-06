using Content.Goobstation.Shared.MisandryBox.JobObjective;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Shared.GameTicking;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.MisandryBox.JobObjective;

/// <summary>
/// Manages crew members having objectives
/// </summary>
public sealed class JobObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ObjectivesSystem _obj = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private const string Rule = "JobObjectiveRule";

    private readonly List<PendingObjective> _objectives = [];
    private EntityUid? _jobObjectiveRule;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JobObjectiveComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawn);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(JobsAssigned);
        SubscribeLocalEvent<JobObjectiveRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnding);
    }

    // Option 1: Making this system GameRuleSystem<JobObjectiveRuleComponent> and adding JobObjectiveRule to every preset
    // Option 2: This
    // We need the gamerule so that we have the objectives show up in the round end window
    private void OnRoundStarting(RoundStartingEvent ev)
    {
        _objectives.Clear();
        _ticker.AddGameRule(Rule);
    }

    private void OnRoundEnding(RoundRestartCleanupEvent ev)
    {
        _objectives.Clear();

        if (_jobObjectiveRule.HasValue)
        {
            Del(_jobObjectiveRule.Value);
            _jobObjectiveRule = null;
        }
    }

    private void OnPlayerSpawn(Entity<JobObjectiveComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mind, out var comp))
            return;

        if (_ticker.RunLevel == GameRunLevel.InRound)
            TryAssignObjectives(mind, comp, ent.Comp);

        _objectives.Add(new PendingObjective(mind, comp, ent.Comp));
    }

    private void JobsAssigned(RulePlayerJobsAssignedEvent ev)
    {
        foreach (var obj in _objectives)
            TryAssignObjectives(obj.Mind, obj.Comp, obj.Objective);
    }

    private void OnObjectivesTextGetInfo(Entity<JobObjectiveRuleComponent> rule, ref ObjectivesTextGetInfoEvent args)
    {
        args.AgentName = Loc.GetString("job-objectives-round-end-crew-name");

        foreach (var objective in _objectives)
        {
            if (!TryComp<MindComponent>(objective.Mind, out var mindComp))
                continue;

            var name = mindComp.CharacterName ?? "Unknown";
            args.Minds.Add((objective.Mind, name));
        }
    }

    private bool TryAssignObjectives(EntityUid mind, MindComponent comp, JobObjectiveComponent objectiveComp)
    {
        var allAssigned = true;

        foreach (var objectiveProto in objectiveComp.Objectives)
        {
            var obj = _obj.TryCreateObjective(mind, comp, objectiveProto);

            if (obj is null)
            {
                Log.Warning($"Failed to create objective {objectiveProto}");
                allAssigned = false;
                continue;
            }

            _mind.AddObjective(mind, comp, obj.Value);
        }

        return allAssigned;
    }
}

public readonly record struct PendingObjective(EntityUid Mind, MindComponent Comp, JobObjectiveComponent Objective);
