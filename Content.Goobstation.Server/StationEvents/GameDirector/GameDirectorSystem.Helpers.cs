using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedJobSystem _jobs = default!;

    /// <summary>
    ///   Count the active players and ghosts on the server.
    ///   Players gates which stories and events are available
    ///   Ghosts can be used to gate certain events (which require ghosts to occur)
    /// </summary>
    private PlayerCount CountActivePlayers()
    {
        var count = new PlayerCount();
        foreach (var player in _playerManager.Sessions)
        {
            if (player.AttachedEntity is not { } entity)
                continue;

            count.InGame++;

            if (HasComp<GhostComponent>(entity))
            {
                count.Ghosts++;
                continue;
            }

            var ev = new GetCharactedDeadIcEvent();
            RaiseLocalEvent(entity, ref ev);
            if (ev.Dead is true || _mobState.IsDead(entity))
                continue;

            count.Players++;

            if (!_mind.TryGetMind(entity, out var mindId, out _)
                || !_jobs.MindTryGetJobId(mindId, out var jobId)
                || jobId is not { } job
                || !_jobs.TryGetAllDepartments(job.Id, out var departments))
                continue;

            foreach (var department in departments)
                count.Departments[department.ID] = count.Departments.GetValueOrDefault(department.ID) + 1;
        }

        count.Players += _event.PlayerCountBias;
        count.InGame += _event.PlayerCountBias;

        return count;
    }

    /// <summary>
    /// Gets the player count for antag selection (debug or actual)
    /// </summary>
    private int GetPlayerCount()
    {
#if DEBUG
        return _gameDirectorDebugPlayerCount;
#else
        return GetTotalPlayerCount(_playerManager.Sessions);
#endif
    }

    /// <summary>
    ///   Count all the players on the server.
    /// </summary>
    private int GetTotalPlayerCount(IList<ICommonSession> pool)
    {
        var count = 0;
        foreach (var session in pool)
        {
            if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
                continue;

            count++;
        }

        return count + _event.PlayerCountBias;
    }
}
