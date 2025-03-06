using System;
using Content.Shared.Eui;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared._Goobstation.ServerCurrency;

namespace Content.Shared._Goobstation.ServerCurrency.UI
{
    [Serializable, NetSerializable]
    public sealed class CurrencyEuiState : EuiStateBase
    {

    }
    public static class CurrencyEuiMsg
    {
        [Serializable, NetSerializable]
        public sealed class Close : EuiMessageBase
        {
        }

        [Serializable, NetSerializable]
        public sealed class Buy : EuiMessageBase
        {
            public ProtoId<TokenListingPrototype> TokenId;
        }
    }
}
