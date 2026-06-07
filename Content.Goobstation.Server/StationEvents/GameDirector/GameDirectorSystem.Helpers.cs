using System.Linq;
using Content.Shared.Mind;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{

    /// <summary>
    ///   Count the active players and ghosts on the server.
    ///   Players gates which stories and events are available
    ///   Ghosts can be used to gate certain events (which require ghosts to occur)
    /// </summary>
    private PlayerCount CountActivePlayers()
    {
        var allPlayers = Enumerable.ToList<ICommonSession>(_playerManager.Sessions);
        var count = new PlayerCount();
        foreach (var player in allPlayers)
        {
            if (player.AttachedEntity == null)
                continue;
            var ev = new GetCharactedDeadIcEvent();
            RaiseLocalEvent(player.AttachedEntity.Value, ref ev);
            if (ev.Dead is not true)
                count.Players++;
            else
                count.Ghosts++;
        }

        count.Players += _event.PlayerCountBias;

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
