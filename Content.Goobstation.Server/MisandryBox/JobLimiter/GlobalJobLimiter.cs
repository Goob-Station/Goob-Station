using System.Linq;
using Content.Goobstation.Shared.MisandryBox.JobLimiter;
using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MisandryBox.JobLimiter;

/// <summary>
/// Limits available passenger job slots depending on taken security slots
/// </summary>
public sealed class GlobalJobLimiterSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;

    private List<JobLimitRulePrototype> _sortedRules = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnJobsAssigned);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        LoadLimitRules();
    }

    private void OnStationInitialized(StationInitializedEvent ev)
    {
        if (!HasComp<StationJobsComponent>(ev.Station))
            return;

        EnsureComp<JobLimiterComponent>(ev.Station);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId is null)
            return;

        var comp = Comp<JobLimiterComponent>(ev.Station);
        comp.JobCounts[ev.JobId] = (comp.JobCounts[ev.JobId] ?? 0) + 1;

        if (ev.LateJoin)
            UpdateAllStationSlots();
    }

    private void OnJobsAssigned(RulePlayerJobsAssignedEvent ev)
    {
        UpdateAllStationSlots();
        LockLimiting();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (ev.WasModified<JobLimitRulePrototype>())
        {
            LoadLimitRules();
            UpdateAllStationSlots();
        }
    }

    private void LockLimiting()
    {
        var query = EntityQueryEnumerator<JobLimiterComponent>();
        while (query.MoveNext(out var comp))
        {
            comp.Active = true;
        }
    }

    private void UpdateAllStationSlots()
    {
        var query = EntityQueryEnumerator<JobLimiterComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            UpdateStationSlots(ent, comp);
        }
    }

    private void UpdateStationSlots(EntityUid station, JobLimiterComponent comp)
    {
        foreach (var rule in _sortedRules)
        {
            var controllingJobCount = 0;

            foreach (var controllingJob in rule.ControllingJobs)
            {
                controllingJobCount += comp.JobCounts[controllingJob] ?? 0;
            }

            var currentLimitedCount = comp.JobCounts[rule.LimitedJob] ?? 0;
            var maxAllowed = CalculateMaxAllowed(station, controllingJobCount, rule);
            var availableSlots = Math.Max(0, maxAllowed - currentLimitedCount);

            _stationJobs.TrySetJobSlot(station, rule.LimitedJob, availableSlots);
        }
    }

    private int CalculateMaxAllowed(EntityUid station, int controllingCount, JobLimitRulePrototype rule)
    {
        if (controllingCount < rule.MinimumControlling)
            return 0;

        var ratioAllowed = (int)Math.Floor(controllingCount * rule.Ratio);

        // Use absoluteMax if specified, otherwise use station's job slot limit, otherwise just allowed ratio
        var maxCap = rule.AbsoluteMax ?? GetStationJobLimit(station, rule.LimitedJob) ?? ratioAllowed;

        return Math.Min(ratioAllowed, maxCap);
    }

    private int? GetStationJobLimit(EntityUid station, ProtoId<JobPrototype> jobId)
    {
        var rstart = _stationJobs.GetRoundStartJobs(station);

        foreach (
            var job in rstart
                .Where(job => job.Key == jobId))
        {
            return job.Value;
        }

        return null;
    }

    private void LoadLimitRules()
    {
        _sortedRules = _prototypeManager
            .EnumeratePrototypes<JobLimitRulePrototype>()
            .OrderByDescending(r => r.Priority)
            .ToList();
    }
}
