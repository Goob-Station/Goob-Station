using System.Linq;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Host)]
internal sealed class SetPatronCommand : LocalizedCommands
{
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override string Command => "setpatron";

    public override string Description => "Assign a debug patron tier to a player";

    public override string Help => "Usage: setpatron <player> <tierId|clear>\n" +
                                    "Example: setpatron \"John Doe\" captain\n" +
                                    "Example: setpatron username clear";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Usage: setpatron <player> <tierId|clear>");
            shell.WriteError("Example: setpatron \"John Doe\" captain");
            shell.WriteError("Example: setpatron username clear");
            return;
        }

        var targetName = args[0];
        var tierId = args[1];

        ICommonSession? targetSession = null;
        foreach (var session in _playerManager.Sessions)
        {
            if (session.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
            {
                targetSession = session;
                break;
            }
        }

        if (targetSession == null)
        {
            shell.WriteError($"Player '{targetName}' not found.");
            return;
        }

        var userId = targetSession.UserId;

        if (tierId.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            _linkAccount.AssignFauxPatron(userId, null);
            shell.WriteLine($"Faux patron cleared for {targetSession.Name}.");
            return;
        }

        var tiers = _linkAccount.GetAllFauxTiers();
        if (!tiers.ContainsKey(tierId))
        {
            shell.WriteError($"Tier '{tierId}' not found. Use 'listpatrontiers' to see available tiers.");
            return;
        }

        _linkAccount.AssignFauxPatron(userId, tierId);
        shell.WriteLine($"Faux patron tier '{tierId}' assigned to {targetSession.Name}.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var playerNames = _playerManager.Sessions.Select(s => s.Name);
            return CompletionResult.FromHintOptions(playerNames, "<player>");
        }

        if (args.Length == 2)
        {
            var tiers = _linkAccount.GetAllFauxTiers();
            var options = tiers.Keys.Append("clear");
            return CompletionResult.FromHintOptions(options, "<tierId|clear>");
        }

        return CompletionResult.Empty;
    }
}
