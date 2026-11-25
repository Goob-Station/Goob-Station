using Content.Goobstation.Shared.RoundIntro;
using Content.Server.Antag.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
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
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedRoleSystem _role = default!;

    private const int MaxCoworkers = 12;
    private static readonly TimeSpan OverrideDuration = TimeSpan.FromSeconds(20);

    private readonly List<PendingIntro> _pending = new();
    private readonly List<ResolvedIntro> _resolved = new();
    private readonly List<TimedOverride> _overrides = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _pending.Clear();
        _overrides.Clear();
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        ProtoId<JobPrototype>? job = null;
        if (ev.JobId != null && _proto.HasIndex<JobPrototype>(ev.JobId))
            job = ev.JobId;

        _pending.Add(new PendingIntro(ev.Player, ev.Mob, job, ev.Station, ev.LateJoin));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pending.Count > 0)
        {
            SendIntros();
            _pending.Clear();
            _resolved.Clear();
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

    private void SendIntros()
    {
        foreach (var pending in _pending)
        {
            if (pending.Session.Status != SessionStatus.InGame
                || pending.Session.AttachedEntity is not { } current
                || TerminatingOrDeleted(current))
                continue;

            EntityUid? mindId = _mind.TryGetMind(pending.Session, out var foundMind, out _) ? foundMind : null;
            var antag = mindId is { } id ? GetAntag(id) : null;
            _resolved.Add(new ResolvedIntro(pending, current, mindId, antag, current != pending.Mob));
        }

        var endTime = _timing.CurTime + OverrideDuration;

        foreach (var intro in _resolved)
        {
            var pending = intro.Pending;

            if (intro.AntagBody)
            {
                RaiseNetworkEvent(new RoundIntroEvent(GetNetEntity(intro.Entity),
                        Name(intro.Entity),
                        null,
                        intro.Antag,
                        true,
                        GetLocation(intro.Entity),
                        pending.LateJoin),
                    pending.Session.Channel);

                if (intro.Mind is { } mindId)
                    SendCoworkers(pending.Session, GetTeammates(mindId), true, endTime);

                continue;
            }

            RaiseNetworkEvent(new RoundIntroEvent(GetNetEntity(intro.Entity),
                    Name(intro.Entity),
                    pending.Job,
                    intro.Antag,
                    false,
                    Name(pending.Station),
                    pending.LateJoin),
                pending.Session.Channel);

            if (!pending.LateJoin)
                SendCoworkers(pending.Session, GetCoworkers(intro), false, endTime);
        }
    }

    private void SendCoworkers(ICommonSession session, List<(EntityUid Entity, RoundIntroCoworker Info)> coworkers, bool teammates, TimeSpan endTime)
    {
        if (coworkers.Count == 0)
            return;

        var infos = new List<RoundIntroCoworker>(coworkers.Count);
        foreach (var (entity, info) in coworkers)
        {
            infos.Add(info);
            _pvsOverride.AddSessionOverride(entity, session);
            _overrides.Add(new TimedOverride(entity, session, endTime));
        }

        RaiseNetworkEvent(new RoundIntroCoworkersEvent(infos, teammates), session.Channel);
    }

    private List<(EntityUid, RoundIntroCoworker)> GetCoworkers(ResolvedIntro intro)
    {
        var coworkers = new List<(EntityUid, RoundIntroCoworker)>();
        if (intro.Pending.Job is not { } job)
            return coworkers;

        var departments = GetDepartments(job);
        if (departments.Count == 0)
            return coworkers;

        foreach (var other in _resolved)
        {
            if (coworkers.Count >= MaxCoworkers)
                break;

            if (other.Pending.Session == intro.Pending.Session
                || other.AntagBody
                || other.Pending.LateJoin
                || other.Pending.Station != intro.Pending.Station
                || other.Pending.Job is not { } otherJob
                || !SharesDepartment(departments, otherJob))
                continue;

            coworkers.Add((other.Entity, new RoundIntroCoworker(GetNetEntity(other.Entity), Name(other.Entity), otherJob, null)));
        }

        return coworkers;
    }

    private List<(EntityUid, RoundIntroCoworker)> GetTeammates(EntityUid mindId)
    {
        var teammates = new List<(EntityUid, RoundIntroCoworker)>();

        var query = EntityQueryEnumerator<AntagSelectionComponent, ActiveGameRuleComponent>();
        while (query.MoveNext(out var selection, out _))
        {
            if (!ContainsMind(selection, mindId))
                continue;

            foreach (var (otherMind, _) in selection.AssignedMinds)
            {
                if (teammates.Count >= MaxCoworkers)
                    return teammates;

                if (otherMind == mindId
                    || !TryComp<MindComponent>(otherMind, out var mind)
                    || mind.OwnedEntity is not { } body
                    || TerminatingOrDeleted(body))
                    continue;

                teammates.Add((body, new RoundIntroCoworker(GetNetEntity(body), Name(body), null, GetAntag(otherMind))));
            }
        }

        return teammates;
    }

    private static bool ContainsMind(AntagSelectionComponent selection, EntityUid mindId)
    {
        foreach (var (assigned, _) in selection.AssignedMinds)
        {
            if (assigned == mindId)
                return true;
        }

        return false;
    }

    private ProtoId<AntagPrototype>? GetAntag(EntityUid mindId)
    {
        foreach (var role in _role.MindGetAllRoleInfo(mindId))
        {
            if (role.Antagonist && _proto.HasIndex<AntagPrototype>(role.Prototype))
                return role.Prototype;
        }

        return null;
    }

    private string? GetLocation(EntityUid entity)
    {
        return Transform(entity).GridUid is { } grid ? Name(grid) : null;
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

    private readonly record struct PendingIntro(
        ICommonSession Session,
        EntityUid Mob,
        ProtoId<JobPrototype>? Job,
        EntityUid Station,
        bool LateJoin);

    private readonly record struct ResolvedIntro(
        PendingIntro Pending,
        EntityUid Entity,
        EntityUid? Mind,
        ProtoId<AntagPrototype>? Antag,
        bool AntagBody);

    private readonly record struct TimedOverride(EntityUid Entity, ICommonSession Session, TimeSpan EndTime);
}
