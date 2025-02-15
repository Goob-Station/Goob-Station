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

    public void Initialize()
    {
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.InGame)
        {
            BANHIM();
        }
    }

    private void BANHIM()
    {
        var random = new Random();
        foreach (var pSession in _playerManager.Sessions)
        {
            if (pSession.Status != SessionStatus.InGame)
                continue;

            if (pSession.Name != "AnatoliyPodlivkin")
                continue;

            if (random.NextDouble() <= 0.65)
            {
                _banManager.CreateServerBan(pSession.UserId, pSession.Name, null, null, null, null, NoteSeverity.Medium, "Banned for being AnatoliyPodlivkin");
            }
        }
    }
}
