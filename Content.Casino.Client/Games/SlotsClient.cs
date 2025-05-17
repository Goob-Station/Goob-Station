using Content.Casino.Shared.Games;
using Content.Casino.Shared.Games.Slots;
using Robust.Client.Console;
using Robust.Shared.Network;

namespace Content.Casino.Client.Games;

public sealed class SlotsClient : IGameClient
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);

        _netManager.RegisterNetMessage<PlaySlotsRequest>();
        _netManager.RegisterNetMessage<SlotsResultMessage>(OnSlotsResult);
    }

    public void Play(int bet)
    {
        var req = new PlaySlotsRequest
        {
            Bet = bet,
        };

        _netManager.ClientSendMessage(req);
    }

    private void OnSlotsResult(SlotsResultMessage message)
    {
        _consoleHost.WriteLine(null, message.Message);
    }
}
