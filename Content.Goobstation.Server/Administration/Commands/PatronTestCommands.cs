using System.Linq;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Shared._RMC14.LinkAccount;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronTestAddCommand : LocalizedCommands
{
    private static readonly string[] BoolOptions = ["true", "false"];

    [Dependency] private readonly LinkAccountManager _linkAccount = default!;

    public override string Command => "patrontest:add";

    public override string Description => "Create a debug patron tier for testing";

    public override string Help => "Usage: patrontest:add <tierId> <tierName> <icon?> <credits?> <ghostcolor?> <ghostcosmetics?> <ghostparticles?> <lobbymessage?> <shoutout?>\n" +
                                    "Example: patrontest:add captain Captain JobIconCaptain true true true true true true";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Usage: patrontest:add <tierId> <tierName> <icon?> <credits?> <ghostcolor?> <ghostcosmetics?> <ghostparticles?> <lobbymessage?> <shoutout?>");
            shell.WriteError("Example: patrontest:add captain \"Captain\" JobIconCaptain true true true true true true");
            return;
        }

        var tierId = args[0];
        var tierName = args[1];
        var icon = args.Length > 2 && !string.IsNullOrEmpty(args[2]) ? args[2] : null;
        var showOnCredits = args.Length > 3 && bool.TryParse(args[3], out var credits) && credits;
        var ghostColor = args.Length > 4 && bool.TryParse(args[4], out var ghost) && ghost;
        var ghostCosmetics = args.Length > 5 && bool.TryParse(args[5], out var cosmetics) && cosmetics;
        var ghostParticles = args.Length > 6 && bool.TryParse(args[6], out var particles) && particles;
        var lobbyMessage = args.Length > 7 && bool.TryParse(args[7], out var lobby) && lobby;
        var roundEndShoutout = args.Length > 8 && bool.TryParse(args[8], out var shoutout) && shoutout;

        var tier = new SharedRMCPatronTier(
            ShowOnCredits: showOnCredits,
            GhostColor: ghostColor,
            GhostCosmetics: ghostCosmetics,
            GhostParticles: ghostParticles,
            LobbyMessage: lobbyMessage,
            RoundEndShoutout: roundEndShoutout,
            Tier: tierName,
            Icon: icon
        );

        _linkAccount.AddFauxTier(tierId, tier);

        shell.WriteLine($"Faux patron tier '{tierId}' created:");
        shell.WriteLine($"  Name: {tierName}");
        shell.WriteLine($"  Icon: {icon ?? "None"}");
        shell.WriteLine($"  Credits: {showOnCredits}");
        shell.WriteLine($"  Ghost Color: {ghostColor}");
        shell.WriteLine($"  Ghost Cosmetics: {ghostCosmetics}");
        shell.WriteLine($"  Ghost Particles: {ghostParticles}");
        shell.WriteLine($"  Lobby Message: {lobbyMessage}");
        shell.WriteLine($"  Shoutout: {roundEndShoutout}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint("<tierId>"),
            2 => CompletionResult.FromHint("<tierName>"),
            3 => CompletionResult.FromHint("<icon?>"),
            4 => CompletionResult.FromHintOptions(BoolOptions, "<credits?>"),
            5 => CompletionResult.FromHintOptions(BoolOptions, "<ghostcolor?>"),
            6 => CompletionResult.FromHintOptions(BoolOptions, "<ghostcosmetics?>"),
            7 => CompletionResult.FromHintOptions(BoolOptions, "<ghostparticles?>"),
            8 => CompletionResult.FromHintOptions(BoolOptions, "<lobbymessage?>"),
            9 => CompletionResult.FromHintOptions(BoolOptions, "<shoutout?>"),
            _ => CompletionResult.Empty
        };
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronTestSetCommand : LocalizedCommands
{
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override string Command => "patrontest:set";

    public override string Description => "Assign a debug patron tier to a player";

    public override string Help => "Usage: patrontest:set <player> <tierId|clear>\n" +
                                    "Example: patrontest:set \"John Doe\" captain\n" +
                                    "Example: patrontest:set username clear";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Usage: patrontest:set <player> <tierId|clear>");
            shell.WriteError("Example: patrontest:set \"John Doe\" captain");
            shell.WriteError("Example: patrontest:set username clear");
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
            shell.WriteError($"Tier '{tierId}' not found. Use 'patrontest:list' to see available tiers.");
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

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronTestListCommand : LocalizedCommands
{
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override string Command => "patrontest:list";

    public override string Description => "List all debug patron tiers and their assignments";

    public override string Help => "Usage: patrontest:list\n" +
                                    "Shows all defined patron tiers and which players have them assigned.";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var tiers = _linkAccount.GetAllFauxTiers();
        var assignments = _linkAccount.GetAllFauxPatronAssignments();

        if (tiers.Count == 0)
        {
            shell.WriteLine("No faux patron tiers defined.");
            shell.WriteLine("Use 'patrontest:add' to create one.");
            return;
        }

        shell.WriteLine($"Active Debug Patrons ({tiers.Count}):");
        shell.WriteLine("=====================================");

        foreach (var (tierId, tier) in tiers)
        {
            shell.WriteLine($"[{tierId}] {tier.Tier}");
            shell.WriteLine($"  Icon: {tier.Icon ?? "None"}");
            shell.WriteLine($"  Credits: {tier.ShowOnCredits} | Ghost Color: {tier.GhostColor}");
            shell.WriteLine($"  Ghost Cosmetics: {tier.GhostCosmetics} | Ghost Particles: {tier.GhostParticles}");
            shell.WriteLine($"  Lobby Message: {tier.LobbyMessage} | Shoutout: {tier.RoundEndShoutout}");

            var assignedUsers = assignments.Where(kvp => kvp.Value == tierId).ToList();
            if (assignedUsers.Count > 0)
            {
                shell.WriteLine("  Assigned to:");
                foreach (var (userId, _) in assignedUsers)
                {
                    var playerName = "Unknown";
                    if (_playerManager.TryGetSessionById(userId, out var session))
                    {
                        playerName = session.Name;
                    }
                    shell.WriteLine($"    - {playerName}");
                }
            }
            shell.WriteLine("");
        }
    }
}
