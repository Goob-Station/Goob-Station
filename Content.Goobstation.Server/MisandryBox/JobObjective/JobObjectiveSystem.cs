using System.Linq;
using Content.Goobstation.Shared.MisandryBox.JobObjective;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.MisandryBox.JobObjective;

public sealed class JobObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ObjectivesSystem _obj = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private const string Rule = "JobObjectiveRule";

    private readonly List<QueuedObjective> _queuedObjectives = [];
    private EntityUid? _jobObjectiveRule;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<JobObjectiveRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnding);
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        _queuedObjectives.Clear();
        _jobObjectiveRule = Spawn(Rule, MapCoordinates.Nullspace);
        _ticker.StartGameRule(_jobObjectiveRule.Value);
    }

    private void OnRoundEnding(RoundRestartCleanupEvent ev)
    {
        _queuedObjectives.Clear();

        if (_jobObjectiveRule.HasValue)
        {
            Del(_jobObjectiveRule.Value);
            _jobObjectiveRule = null;
        }
    }

    public void QueueObjectives(EntityUid mob, List<string> objectives)
    {
        _queuedObjectives.Add(new QueuedObjective(mob, objectives));
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (!_mind.TryGetMind(ev.Mob, out var mind, out var comp))
            return;

        var queuedForMob = _queuedObjectives.Where(q => q.Mob == ev.Mob).ToList();

        foreach (var queued in queuedForMob)
        {
            if (TryAssignObjectives(mind, comp, queued.Objectives))
                AddTrackedMind(mind, comp);

            _queuedObjectives.Remove(queued);
        }
    }

    private void OnObjectivesTextGetInfo(Entity<JobObjectiveRuleComponent> rule, ref ObjectivesTextGetInfoEvent args)
    {
        args.AgentName = Loc.GetString("job-objectives-round-end-crew-name");

        foreach (var (mind, comp) in rule.Comp.TrackedMinds)
        {
            args.Minds.Add((mind, comp.CharacterName ?? "Unknown"));
        }
    }

    private void AddTrackedMind(EntityUid mind, MindComponent mindComp)
    {
        if (!_jobObjectiveRule.HasValue || !TryComp<JobObjectiveRuleComponent>(_jobObjectiveRule.Value, out var ruleComp))
            return;

        ruleComp.TrackedMinds.Add((mind, mindComp));
    }

    private bool TryAssignObjectives(EntityUid mind, MindComponent comp, List<string> objectives)
    {
        var allAssigned = true;

        foreach (var objectiveProto in objectives)
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

public readonly record struct QueuedObjective(EntityUid Mob, List<string> Objectives);
