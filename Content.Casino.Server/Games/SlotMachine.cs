using System;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Games.Slots;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Casino.Server.Games;

public sealed class SlotMachine : ICasinoGame
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IServerCasinoManager _casinoManager = default!;

    public string Id => "slots";
    public string Name => "Slot Machine";
    public string Description => "Today's your lucky day";

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
        _netMan.RegisterNetMessage<PlaySlotsRequest>(OnPlaySlotsRequest);
        _netMan.RegisterNetMessage<SlotsResultMessage>();
    }

    private async void OnPlaySlotsRequest(PlaySlotsRequest message)
    {
        var session = _player.GetSessionByChannel(message.MsgChannel);
        var bet = message.Bet;

        if (!_casinoManager.TryCreateStake(session, bet, out var escrow))
        {
            var failMsg = new SlotsResultMessage
            {
                Won = false,
                Payout = 0,
                Message = "Insufficient funds to place bet."
            };
            _netMan.ServerSendMessage(failMsg, session.Channel);
            return;
        }

        var result = await Play(session, bet);

        _casinoManager.ResolveStake(escrow.Value.GameId, result);

        var resultMsg = new SlotsResultMessage
        {
            Won = result.Won,
            Payout = result.Payout,
            Message = result.Message
        };
        _netMan.ServerSendMessage(resultMsg, session.Channel);
    }

    public Task<GameResult> Play(ICommonSession session, int bet)
    {
        var symbols = new SlotSymbol[3];
        for (var i = 0; i < symbols.Length; i++)
        {
            symbols[i] = (SlotSymbol)_random.Next(0, Enum.GetValues<SlotSymbol>().Length);
        }

        var payout = symbols.GetPayout(bet);
        var won = payout > 0;

        var symbolsText = $"[ {symbols[0]} | {symbols[1]} | {symbols[2]} ]";
        string message;

        if (payout > bet)
            message = $"You won {payout} goobcoins! {symbolsText}";
        else if (payout == bet)
            message = $"Pushed! {symbolsText}";
        else
            message = $"You lost! {symbolsText}";

        return Task.FromResult(new GameResult(won, payout, message));
    }

}
