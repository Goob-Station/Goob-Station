using Content.Server.Administration.Managers;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Content.Shared.Database;

namespace Content.Server._Goobstation.AntiJohnStation;

public sealed class AntiJohnStation
{
    [Dependency] private readonly IBanManager _banManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private void BANHIM()
    {
        var random = new Random();
        foreach (var pSession in Filter.GetAllPlayers())
        {
            if (pSession.Status != SessionStatus.InGame)
                continue;

            if (pSession.Data.UserName != "AnatoliyPodlivkin")
                continue;

            if (random.NextDouble() <= 0.65)
            {
                _banManager.CreateServerBan(pSession.UserId, pSession.Data.UserName, null, null, null, null, NoteSeverity.Medium, "Banned for rolling antags");
            }
        }
    }
}
