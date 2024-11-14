using Content.Server._Goobstation.ServerCurrency.Systems;
using Content.Shared.Administration;
using Content.Server.Administration;
using Robust.Shared.Console;
using Robust.Server.Player;

namespace Content.Server._Goobstation.ServerCurrency.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AddServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencySystem _Currency = default!;

        public string Command => "addservercurrency";
        public string Description => "Adds server currency to a player.";
        public string Help => "Usage: addservercurrency <player> <value>";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError("Unable to find a player by that name.");
                return;
            }

            if (!int.TryParse(args[1], out int currency))
            {
                shell.WriteError("Value must be an integer!");
                return;
            }

            var newCurrency = await _Currency.AddCurrency(targetPlayer, currency);
            shell.WriteLine($"Currency added, {args[0]} now has {newCurrency}");
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class RemoveServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencySystem _Currency = default!;

        public string Command => "remservercurrency";
        public string Description => "Removes server currency to a player.";
        public string Help => "Usage: remservercurrency <player> <value>";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError("Unable to find a player by that name.");
                return;
            }

            if (!int.TryParse(args[1], out int currency))
            {
                shell.WriteError("Value must be an integer!");
                return;
            }

            var newCurrency = await _Currency.SubtractCurrency(targetPlayer, currency);
            shell.WriteLine($"Currency removed, {args[0]} now has {newCurrency}");
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencySystem _Currency = default!;

        public string Command => "setservercurrency";
        public string Description => "Sets server currency for a player.";
        public string Help => "Usage: setservercurrency <player> <value>";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError("Unable to find a player by that name.");
                return;
            }

            if (int.TryParse(args[1], out int currency))
            {
                shell.WriteError("Value must be an integer!");
                return;
            }

            var newCurrency = await _Currency.SetCurrency(targetPlayer, currency);
            shell.WriteLine($"Currency set, {args[0]} now has {newCurrency}");
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class GetServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencySystem _Currency = default!;

        public string Command => "getservercurrency";
        public string Description => "Gets the server currency of a player.";
        public string Help => "Usage: getservercurrency <player>";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError("Unable to find a player by that name.");
                return;
            }

            var currency = await _Currency.GetCurrency(targetPlayer);
            shell.WriteLine($"{args[0]} has {currency}");
        }
    }
}
