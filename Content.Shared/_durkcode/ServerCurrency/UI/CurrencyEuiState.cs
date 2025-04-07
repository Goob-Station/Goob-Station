using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._durkcode.ServerCurrency.UI
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
