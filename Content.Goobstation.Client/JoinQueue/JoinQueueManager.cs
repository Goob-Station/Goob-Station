using Content.Goobstation.Shared.JoinQueue;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.JoinQueue;

public sealed class JoinQueueManager
{
    [Dependency] private readonly IClientNetManager _net = default!;
    [Dependency] private readonly IStateManager _state = default!;


    public void Initialize()
    {
        _net.RegisterNetMessage<QueueUpdateMessage>(OnQueueUpdate);
    }


    private void OnQueueUpdate(QueueUpdateMessage msg)
    {
        if (_state.CurrentState is not QueueState)
            _state.RequestStateChange<QueueState>();

        ((QueueState) _state.CurrentState).OnQueueUpdate(msg);
    }
}
