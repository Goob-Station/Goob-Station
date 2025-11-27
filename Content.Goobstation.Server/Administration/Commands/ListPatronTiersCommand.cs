using System.Linq;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Host)]
internal sealed class ListPatronTiersCommand : LocalizedCommands
{
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override string Command => "listpatrontiers";

    public override string Description => "List all debug patron tiers and their assignments";

    public override string Help => "Usage: listpatrontiers\n" +
                                    "Shows all defined patron tiers and which players have them assigned.";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var tiers = _linkAccount.GetAllFauxTiers();
        var assignments = _linkAccount.GetAllFauxPatronAssignments();

        if (tiers.Count == 0)
        {
            shell.WriteLine("No faux patron tiers defined.");
            shell.WriteLine("Use 'addpatrontier' to create one.");
            return;
        }

        shell.WriteLine($"Active Debug Patrons ({tiers.Count}):");
        shell.WriteLine("=====================================");

        foreach (var (tierId, tier) in tiers)
        {
            shell.WriteLine($"[{tierId}] {tier.Tier}");
            shell.WriteLine($"  Icon: {tier.Icon ?? "None"}");
            shell.WriteLine($"  Credits: {tier.ShowOnCredits} | Ghost Color: {tier.GhostColor}");
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
