// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency.UI;
using Content.Server.Administration.Notes;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared.Eui;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.ServerCurrency.UI
{
    public sealed class CurrencyEui : BaseEui
    {
        private static readonly ProtoId<TokenListingPrototype> AntagTokenProtoId = "HighTierAntagToken";

        [Dependency] private readonly ICommonCurrencyManager _currencyMan = default!;
        [Dependency] private readonly IAdminNotesManager _notesMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IServerDbManager _dbMan = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

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

            if (!_protoMan.TryIndex(tokenId, out var token))
                return;

            if (balance < token.Price)
                return;

            if (tokenId == AntagTokenProtoId)
            {
                var cap = _cfg.GetCVar(GoobCVars.AntagTokenCap);
                var currentCount = await _dbMan.GetAntagTokenCount(Player.UserId);
                if (currentCount >= cap)
                    return;

                await _dbMan.IncrementAntagTokens(Player.UserId, 1, cap);
                _currencyMan.RemoveCurrency(Player.UserId, token.Price);
            }
            else // if anything dies go to notes
            {
                await _notesMan.AddAdminRemark(Player, Player.UserId, 0,
                    Loc.GetString(token.AdminNote), 0, false, null);
                _currencyMan.RemoveCurrency(Player.UserId, token.Price);
            }
        }
    }
}
