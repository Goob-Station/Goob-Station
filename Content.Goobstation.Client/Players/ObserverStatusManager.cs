using Content.Goobstation.Shared.Players;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.Players;

/// <summary>
/// Client-side manager for tracking whether the player joined as an observer.
/// </summary>
public sealed class ObserverStatusManager : IObserverStatusManager, IPostInjectInit
{
    [Dependency] private readonly IClientNetManager _net = default!;

    public bool JoinedAsObserver { get; private set; }
    public bool IsAdmin { get; private set; }

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<MsgObserverStatus>(HandleObserverStatus);
    }

    private void HandleObserverStatus(MsgObserverStatus message)
    {
        JoinedAsObserver = message.JoinedAsObserver;
        IsAdmin = message.IsAdmin;
    }
}
