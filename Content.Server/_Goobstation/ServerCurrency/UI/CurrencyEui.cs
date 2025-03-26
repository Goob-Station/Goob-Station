using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared._Goobstation.ServerCurrency;
using Content.Shared._Goobstation.ServerCurrency.UI;
using Robust.Shared.Player;
using Content.Server.Administration.Notes;
using Content.Server._Goobstation.ServerCurrency;
using Robust.Shared.Prototypes;
namespace Content.Server._Goobstation.ServerCurrency.UI
{
    public sealed class CurrencyEui : BaseEui
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;
        [Dependency] private readonly IAdminNotesManager _notesMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        public CurrencyEui()
        {
            IoCManager.InjectDependencies(this);
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new CurrencyEuiState();
        }


        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);
            switch (msg)
            {
                case CurrencyEuiMsg.Buy Buy:

                    BuyToken(Buy.TokenId, Player);
                    StateDirty();
                    break; //grrr fix formatting
            }
        }

        private async void BuyToken(ProtoId<TokenListingPrototype> tokenId, ICommonSession playerName)
        {
            var balance = _currencyMan.GetBalance(Player.UserId);

            if (!_protoMan.TryIndex<TokenListingPrototype>(tokenId, out var token))
                return;

            if (balance < token.Price)
                return;

            await _notesMan.AddAdminRemark(Player, Player.UserId, 0, 
                Loc.GetString(token.AdminNote), 0, false, null);
            _currencyMan.RemoveCurrency(Player.UserId, token.Price);
        }
    }
}
