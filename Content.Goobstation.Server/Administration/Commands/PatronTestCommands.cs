using System.Linq;
using Content.Goobstation.Common.Patrons;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Host)]
internal sealed partial class PatronTestSetCommand : LocalizedCommands
{
    [Dependency] private LinkAccountManager _linkAccount = default!;
    [Dependency] private ISharedPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    public override string Command => "patrontest:set";

    public override string Description => "Temporarily assign a patron tier to a player for testing";

    public override string Help => "Usage: patrontest:set <player> <tier|clear>\n" +
                                    "Example: patrontest:set \"John Doe\" PatronTierCaptain\n" +
                                    "Example: patrontest:set username clear";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Help);
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
            _linkAccount.SendPatronStatus(targetSession);
            shell.WriteLine($"Faux patron cleared for {targetSession.Name}.");
            return;
        }

        if (!_prototypes.HasIndex<PatronTierPrototype>(tierId))
        {
            shell.WriteError($"Tier '{tierId}' not found.");
            return;
        }

        _linkAccount.AssignFauxPatron(userId, tierId);
        _linkAccount.SendPatronStatus(targetSession);
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
            var options = _prototypes.EnumeratePrototypes<PatronTierPrototype>().Select(t => t.ID).Append("clear");
            return CompletionResult.FromHintOptions(options, "<tier|clear>");
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed partial class PatronTestListCommand : LocalizedCommands
{
    [Dependency] private LinkAccountManager _linkAccount = default!;
    [Dependency] private ISharedPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    public override string Command => "patrontest:list";

    public override string Description => "List all patron tiers and faux tier assignments";

    public override string Help => "Usage: patrontest:list";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var assignments = _linkAccount.GetAllFauxPatronAssignments();

        foreach (var tier in _prototypes.EnumeratePrototypes<PatronTierPrototype>().OrderBy(t => t.Priority))
        {
            shell.WriteLine($"[{tier.ID}] {Loc.GetString(tier.Name)} (db id {tier.DatabaseId}, priority {tier.Priority})");
            shell.WriteLine($"  Icon: {tier.Icon?.Id ?? "None"} | OOC icon tiers: {string.Join(", ", tier.OocIconTiers)}");
            shell.WriteLine($"  Credits: {tier.ShowOnCredits} | Ghost Color: {tier.GhostColor}");
            shell.WriteLine($"  Ghost Cosmetics: {tier.GhostCosmetics} | Ghost Particles: {tier.GhostParticles}");
            shell.WriteLine($"  Lobby Message: {tier.LobbyMessage} | Shoutout: {tier.RoundEndShoutout}");

            var assigned = assignments.Where(kvp => kvp.Value == tier.ID).ToList();
            if (assigned.Count > 0)
            {
                shell.WriteLine("  Faux assigned to:");
                foreach (var (userId, _) in assigned)
                {
                    var playerName = _playerManager.TryGetSessionById(userId, out var session) ? session.Name : "Unknown";
                    shell.WriteLine($"    - {playerName}");
                }
            }

            shell.WriteLine("");
        }
    }
}
