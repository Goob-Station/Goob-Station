using Content.Goobstation.Shared.RoundIntro;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Server.GameStates;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.RoundIntro;

public sealed partial class RoundIntroSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private PvsOverrideSystem _pvsOverride = default!;

    private const int MaxCoworkers = 12;
    private static readonly TimeSpan OverrideDuration = TimeSpan.FromSeconds(20);

    private readonly List<RoundStartSpawn> _roundStartSpawns = new();
    private readonly List<TimedOverride> _overrides = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _roundStartSpawns.Clear();
        _overrides.Clear();
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        ProtoId<JobPrototype>? job = null;
        if (ev.JobId != null && _proto.HasIndex<JobPrototype>(ev.JobId))
            job = ev.JobId;

        RaiseNetworkEvent(new RoundIntroEvent(GetNetEntity(ev.Mob), Name(ev.Mob), job, Name(ev.Station), ev.LateJoin),
            ev.Player.Channel);

        if (!ev.LateJoin)
            _roundStartSpawns.Add(new RoundStartSpawn(ev.Player, ev.Mob, job, ev.Station));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_roundStartSpawns.Count > 0)
        {
            SendCoworkers();
            _roundStartSpawns.Clear();
        }

        if (_overrides.Count == 0)
            return;

        var now = _timing.CurTime;
        for (var i = _overrides.Count - 1; i >= 0; i--)
        {
            var entry = _overrides[i];
            if (entry.EndTime > now)
                continue;

            if (!TerminatingOrDeleted(entry.Entity))
                _pvsOverride.RemoveSessionOverride(entry.Entity, entry.Session);

            _overrides.RemoveAt(i);
        }
    }

    private void SendCoworkers()
    {
        var endTime = _timing.CurTime + OverrideDuration;

        foreach (var spawn in _roundStartSpawns)
        {
            if (spawn.Job is not { } job || spawn.Session.Status != SessionStatus.InGame)
                continue;

            var departments = GetDepartments(job);
            if (departments.Count == 0)
                continue;

            var coworkers = new List<RoundIntroCoworker>();
            foreach (var other in _roundStartSpawns)
            {
                if (coworkers.Count >= MaxCoworkers)
                    break;

                if (other.Session == spawn.Session
                    || other.Station != spawn.Station
                    || other.Job is not { } otherJob
                    || TerminatingOrDeleted(other.Mob)
                    || !SharesDepartment(departments, otherJob))
                    continue;

                coworkers.Add(new RoundIntroCoworker(GetNetEntity(other.Mob), Name(other.Mob), otherJob));
                _pvsOverride.AddSessionOverride(other.Mob, spawn.Session);
                _overrides.Add(new TimedOverride(other.Mob, spawn.Session, endTime));
            }

            if (coworkers.Count > 0)
                RaiseNetworkEvent(new RoundIntroCoworkersEvent(coworkers), spawn.Session.Channel);
        }
    }

    private List<DepartmentPrototype> GetDepartments(ProtoId<JobPrototype> job)
    {
        var departments = new List<DepartmentPrototype>();
        foreach (var department in _proto.EnumeratePrototypes<DepartmentPrototype>())
        {
            if (department.Primary && department.Roles.Contains(job))
                departments.Add(department);
        }

        return departments;
    }

    private static bool SharesDepartment(List<DepartmentPrototype> departments, ProtoId<JobPrototype> job)
    {
        foreach (var department in departments)
        {
            if (department.Roles.Contains(job))
                return true;
        }

        return false;
    }

    private readonly record struct RoundStartSpawn(ICommonSession Session, EntityUid Mob, ProtoId<JobPrototype>? Job, EntityUid Station);

    private readonly record struct TimedOverride(EntityUid Entity, ICommonSession Session, TimeSpan EndTime);
}
