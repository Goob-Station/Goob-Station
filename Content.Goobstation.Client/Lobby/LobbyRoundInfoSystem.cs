using Content.Goobstation.Shared.Lobby;

namespace Content.Goobstation.Client.Lobby;

public sealed partial class LobbyRoundInfoSystem : EntitySystem
{
    public LobbyRoundInfoEvent? Info { get; private set; }

    public event Action<LobbyRoundInfoEvent>? Updated;

    [SubscribeNetworkEvent]
    private void OnRoundInfo(LobbyRoundInfoEvent ev)
    {
        Info = ev;
        Updated?.Invoke(ev);
    }
}
