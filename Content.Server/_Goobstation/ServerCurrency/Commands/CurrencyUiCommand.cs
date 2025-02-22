using Content.Server._Goobstation.ServerCurrency.UI;
using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Goobstation.ServerCurrency.Commands
{
    [AnyCommand]
    public sealed class CurrencyUiCommand : IConsoleCommand
    {
        public string Command => "balanceui";

        public string Description => "Open the currency UI";

        public string Help => $"{Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine("This does not work from the server console.");
                return;
            }

            var eui = IoCManager.Resolve<EuiManager>();
            var ui = new CurrencyEui();
            eui.OpenEui(ui, player);
        }
    }
}
