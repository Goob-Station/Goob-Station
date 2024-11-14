using Content.Server._Goobstation.ServerCurrency;
using Content.Shared.Administration;
using Content.Server.Administration;
using Robust.Shared.Console;
using Robust.Server.Player;

namespace Content.Server._Goobstation.ServerCurrency.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AddServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;

        public string Command => Loc.GetString("server-currency-add-command");
        public string Description => Loc.GetString("server-currency-add-command-description");
        public string Help => Loc.GetString("server-currency-add-command-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (!int.TryParse(args[1], out int currency))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-2"));
                return;
            }

            var newCurrency = _currencyMan.Stringify(_currencyMan.AddCurrency(targetPlayer, currency));
            shell.WriteLine(Loc.GetString("server-currency-command-return", ("player", args[0]), ("amount", newCurrency)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-currency-command-completion-1")),
                2 => CompletionResult.FromHint(Loc.GetString("server-currency-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class RemoveServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;

        public string Command => Loc.GetString("server-currency-remove-command");
        public string Description => Loc.GetString("server-currency-remove-command-description");
        public string Help => Loc.GetString("server-currency-remove-command-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (!int.TryParse(args[1], out int currency))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-2"));
                return;
            }

            var newCurrency = _currencyMan.Stringify(_currencyMan.RemoveCurrency(targetPlayer, currency));
            shell.WriteLine(Loc.GetString("server-currency-command-return", ("player", args[0]), ("amount", newCurrency)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-currency-command-completion-1")),
                2 => CompletionResult.FromHint(Loc.GetString("server-currency-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;

        public string Command => Loc.GetString("server-currency-set-command");
        public string Description => Loc.GetString("server-currency-set-command-description");
        public string Help => Loc.GetString("server-currency-set-command-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (!int.TryParse(args[1], out int currency))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-2"));
                return;
            }

            var newCurrency = _currencyMan.Stringify(_currencyMan.SetBalance(targetPlayer, currency));
            shell.WriteLine(Loc.GetString("server-currency-command-return", ("player", args[0]), ("amount", newCurrency)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-currency-command-completion-1")),
                2 => CompletionResult.FromHint(Loc.GetString("server-currency-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class GetServerCurrencyCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerCurrencyManager _currencyMan = default!;

        public string Command => Loc.GetString("server-currency-get-command");
        public string Description => Loc.GetString("server-currency-get-command-description");
        public string Help => Loc.GetString("server-currency-get-command-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            var currency = _currencyMan.Stringify(_currencyMan.GetBalance(targetPlayer));
            shell.WriteLine(Loc.GetString("server-currency-command-return", ("player", args[0]), ("amount", currency)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-currency-command-completion-1")),
                _ => CompletionResult.Empty
            };
        }
    }
}
