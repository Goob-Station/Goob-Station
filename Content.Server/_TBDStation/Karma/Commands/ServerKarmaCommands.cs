using Content.Shared.Administration;
using Content.Server.Administration;
using Robust.Shared.Console;
using Robust.Server.Player;
using Content.Shared.Chat;
using Content.Server.Chat.Managers;

namespace Content.Server._TBDStation.ServerKarma.Commands
{
    [AnyCommand]
    public sealed class KarmaServerKarmaCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerKarmaManager _KarmaMan = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        public string Command => Loc.GetString("server-karma-karma-command");
        public string Description => Loc.GetString("server-karma-karma-command-description");
        public string Help => Loc.GetString("server-karma-karma-command-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 0)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if(shell.Player is not { } player){
                shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
                return;
            }

            var karma = Loc.GetString("server-karma-karma-command-return",
                ("karma", _KarmaMan.Stringify(_KarmaMan.GetKarma(shell.Player.UserId))));

            _chatManager.ChatMessageToOne(ChatChannel.Local, karma, karma, EntityUid.Invalid, false, shell.Player.Channel);
            shell.WriteLine(karma);
        }
    }

    [AdminCommand(AdminFlags.Host)]
    public sealed class AddServerKarmaCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerKarmaManager _KarmaMan = default!;

        public string Command => Loc.GetString("server-karma-add-command");
        public string Description => Loc.GetString("server-karma-add-command-description");
        public string Help => Loc.GetString("server-karma-add-command-help");

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
                shell.WriteError(Loc.GetString("server-karma-command-error-1"));
                return;
            }

            if (!int.TryParse(args[1], out int Karma))
            {
                shell.WriteError(Loc.GetString("server-karma-command-error-2"));
                return;
            }

            var newKarma = _KarmaMan.Stringify(_KarmaMan.AddKarma(targetPlayer, Karma));
            shell.WriteLine(Loc.GetString("server-karma-command-return", ("player", args[0]), ("karma", newKarma)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-karma-command-completion-1")),
                2 => CompletionResult.FromHint(Loc.GetString("server-karma-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Host)]
    public sealed class RemoveServerKarmaCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerKarmaManager _KarmaMan = default!;

        public string Command => Loc.GetString("server-karma-remove-command");
        public string Description => Loc.GetString("server-karma-remove-command-description");
        public string Help => Loc.GetString("server-karma-remove-command-help");

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
                shell.WriteError(Loc.GetString("server-karma-command-error-1"));
                return;
            }

            if (!int.TryParse(args[1], out int Karma))
            {
                shell.WriteError(Loc.GetString("server-karma-command-error-2"));
                return;
            }

            var newKarma = _KarmaMan.Stringify(_KarmaMan.RemoveKarma(targetPlayer, Karma));
            shell.WriteLine(Loc.GetString("server-karma-command-return", ("player", args[0]), ("karma", newKarma)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-karma-command-completion-1")),
                2 => CompletionResult.FromHint(Loc.GetString("server-karma-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Host)]
    public sealed class SetServerKarmaCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerKarmaManager _KarmaMan = default!;

        public string Command => Loc.GetString("server-karma-set-command");
        public string Description => Loc.GetString("server-karma-set-command-description");
        public string Help => Loc.GetString("server-karma-set-command-help");

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
                shell.WriteError(Loc.GetString("server-karma-command-error-1"));
                return;
            }

            if (!int.TryParse(args[1], out int Karma))
            {
                shell.WriteError(Loc.GetString("server-karma-command-error-2"));
                return;
            }

            var newKarma = _KarmaMan.Stringify(_KarmaMan.SetKarma(targetPlayer, Karma));
            shell.WriteLine(Loc.GetString("server-karma-command-return", ("player", args[0]), ("karma", newKarma)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-karma-command-completion-1")),
                2 => CompletionResult.FromHint(Loc.GetString("server-karma-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Host)]
    public sealed class GetServerKarmaCommand : IConsoleCommand
    {
        [Dependency] private readonly ServerKarmaManager _KarmaMan = default!;

        public string Command => Loc.GetString("server-karma-get-command");
        public string Description => Loc.GetString("server-karma-get-command-description");
        public string Help => Loc.GetString("server-karma-get-command-help");

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
                shell.WriteError(Loc.GetString("server-karma-command-error-1"));
                return;
            }

            var Karma = _KarmaMan.Stringify(_KarmaMan.GetKarma(targetPlayer));
            shell.WriteLine(Loc.GetString("server-karma-command-return", ("player", args[0]), ("karma", Karma)));
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-karma-command-completion-1")),
                _ => CompletionResult.Empty
            };
        }
    }
}
