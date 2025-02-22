using Content.Client.Eui;
using Content.Shared.Administration;
using Robust.Client.UserInterface.Controls;
using Content.Shared._Goobstation.ServerCurrency.UI;
using System.Diagnostics;

namespace Content.Client._Goobstation.ServerCurrency.UI
{
    public class CurrencyEui : BaseEui
    {
        private readonly CurrencyWindow _window;
        public CurrencyEui()
        {
            _window = new CurrencyWindow();
            _window.OnClose += () => SendMessage(new CurrencyEuiMsg.Close());
            _window.OnBuy += OnBuyMsg;
        }

        private void OnBuyMsg(Enum message)
        {
            SendMessage(new CurrencyEuiMsg.Buy
            {
                BuyId = (BuyIdList) message
            });
            SendMessage(new CurrencyEuiMsg.Close());
        }

        public override void Opened()
        {
            _window.OpenCentered();
        }
        public override void Closed()
        {
            _window.Close();
        }
    }
}
