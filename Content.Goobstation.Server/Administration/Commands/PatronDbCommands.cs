using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Common.Patrons;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

internal abstract partial class BasePatronDbCommand : LocalizedCommands
{
    [Dependency] protected IServerDbManager Db = default!;
    [Dependency] protected LinkAccountManager LinkAccount = default!;
    [Dependency] protected IPlayerManager PlayerManager = default!;
    [Dependency] protected IPrototypeManager Prototypes = default!;

    protected PatronTierPrototype? FindTier(IConsoleShell shell, string idOrName)
    {
        foreach (var tier in Prototypes.EnumeratePrototypes<PatronTierPrototype>())
        {
            if (tier.ID.Equals(idOrName, StringComparison.OrdinalIgnoreCase) ||
                Loc.GetString(tier.Name).Equals(idOrName, StringComparison.OrdinalIgnoreCase) ||
                int.TryParse(idOrName, out var dbId) && tier.DatabaseId == dbId)
            {
                return tier;
            }
        }

        shell.WriteError($"Patron tier '{idOrName}' not found.");
        shell.WriteError($"Available tiers: {string.Join(", ", Prototypes.EnumeratePrototypes<PatronTierPrototype>().Select(t => t.ID))}");
        return null;
    }

    protected async Task RefreshPatrons(IEnumerable<Guid> playerIds)
    {
        foreach (var playerId in playerIds)
        {
            if (PlayerManager.TryGetSessionById(new NetUserId(playerId), out var session))
                await LinkAccount.ReloadPatron(session);
        }

        await LinkAccount.RefreshAllPatrons();
        LinkAccount.SendPatronsToAll();
    }

    protected CompletionResult CompleteTiers(string hint, params string[] extra)
    {
        var tiers = Prototypes.EnumeratePrototypes<PatronTierPrototype>().Select(t => t.ID).Concat(extra);
        return CompletionResult.FromHintOptions(tiers, hint);
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed partial class PatronSetCommand : BasePatronDbCommand
{
    public override string Command => "patron:set";

    public override string Description => "Set or remove a player's patron tier in the database";

    public override string Help => "Usage: patron:set <username> <tier|none>\n" +
                                    "Example: patron:set \"John Doe\" PatronTierCaptain\n" +
                                    "Example: patron:set \"John Doe\" none";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length != 2)
            {
                shell.WriteError(Help);
                return;
            }

            var player = await Db.GetPlayerRecordByUserName(args[0]);
            if (player == null)
            {
                shell.WriteError($"Player '{args[0]}' was never seen on this server.");
                return;
            }

            if (args[1].Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                if (!await Db.SetPatron(player.UserId, null))
                {
                    shell.WriteError($"'{player.LastSeenUserName}' is not a patron.");
                    return;
                }

                await RefreshPatrons([player.UserId.UserId]);
                shell.WriteLine($"Removed patron status from '{player.LastSeenUserName}'.");
                return;
            }

            if (FindTier(shell, args[1]) is not { } tier)
                return;

            await Db.SetPatron(player.UserId, tier.DatabaseId);
            await RefreshPatrons([player.UserId.UserId]);
            shell.WriteLine($"Set '{player.LastSeenUserName}' to patron tier '{tier.ID}'.");
        }
        catch (Exception e)
        {
            shell.WriteError($"Error setting patron:\n{e}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var playerNames = PlayerManager.Sessions.Select(s => s.Name);
            return CompletionResult.FromHintOptions(playerNames, "<username>");
        }

        if (args.Length == 2)
            return CompleteTiers("<tier|none>", "none");

        return CompletionResult.Empty;
    }
}
