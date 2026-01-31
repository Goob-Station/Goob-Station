using Content.Goobstation.Shared.Players;
using Content.Shared.Players;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [Dependency] private readonly IServerNetManager _netManager = default!;

    private void InitializeObserverStatus()
    {
        _netManager.RegisterNetMessage<MsgObserverStatus>();
    }

    /// <summary>
    /// Sets the observer status for the player and sends a message to the client.
    /// </summary>
    private void SetObserverStatus(ICommonSession player, bool isObserver)
    {
        var contentData = player.ContentData();
        if (contentData != null)
        {
            contentData.JoinedAsObserver = isObserver;

            var isAdmin = _adminManager.IsAdmin(player);

            var msg = new MsgObserverStatus
            {
                JoinedAsObserver = isObserver,
                IsAdmin = isAdmin
            };
            player.Channel.SendMessage(msg);
        }
    }
}
