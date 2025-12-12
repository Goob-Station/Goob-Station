using System.Linq;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.StationEvents;

public sealed partial class GameDirectorSystem
{
    /// <summary>
    ///   Count the active players and ghosts on the server.
    ///   Players gates which stories and events are available
    ///   Ghosts can be used to gate certain events (which require ghosts to occur)
    /// </summary>
    private PlayerCount CountActivePlayers()
    {
        var allPlayers = _playerManager.Sessions.ToList();
        var count = new PlayerCount();
        foreach (var player in allPlayers)
        {
            if (player.AttachedEntity == null)
                continue;
            // TODO: Consider a custom component here instead of HumanoidAppearanceComponent to represent
            //        "significant enough to count as a whole player"
            if (HasComp<HumanoidAppearanceComponent>(player.AttachedEntity))
                count.Players += 1;
            else if (HasComp<GhostComponent>(player.AttachedEntity))
                count.Ghosts += 1;
        }

        count.Players += _event.PlayerCountBias;

        return count;
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
