using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Administration.Commands;

internal abstract class BasePatronDbCommand : LocalizedCommands
{
    protected static readonly string[] BoolOptions = ["true", "false"];

    [Dependency] protected readonly IServerDbManager Db = default!;
    [Dependency] protected readonly LinkAccountManager LinkAccount = default!;
    [Dependency] protected readonly IPlayerManager PlayerManager = default!;

    protected async Task<RMCPatronTier?> FindTier(IConsoleShell shell, string idOrName)
    {
        var tiers = await Db.GetPatronTiers();
        var tier = int.TryParse(idOrName, out var id)
            ? tiers.FirstOrDefault(t => t.Id == id)
            : tiers.FirstOrDefault(t => string.Equals(t.Name, idOrName, StringComparison.OrdinalIgnoreCase));

        if (tier == null)
        {
            shell.WriteError($"Patron tier '{idOrName}' not found.");
            shell.WriteError(tiers.Count == 0
                ? "There are no patron tiers in the database."
                : $"Available tiers: {string.Join(", ", tiers.Select(t => $"[{t.Id}] {t.Name}"))}");
        }

        return tier;
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

    protected async Task<List<Guid>> GetTierMemberIds(int tierId)
    {
        var patrons = await Db.GetAllPatrons();
        return patrons.Where(p => p.TierId == tierId).Select(p => p.PlayerId).ToList();
    }

    protected async ValueTask<CompletionResult> CompleteTierNames(string hint)
    {
        var tiers = await Db.GetPatronTiers();
        return CompletionResult.FromHintOptions(tiers.Select(t => t.Name), hint);
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronAddCommand : BasePatronDbCommand
{
    public override string Command => "patron:add";

    public override string Description => "Create a patron tier in the database";

    public override string Help => "Usage: patron:add <name> <discordRole> <priority?> <icon?|none> <credits?> <ghostcolor?> <ghostcosmetics?> <ghostparticles?> <lobbymessage?> <shoutout?>\n" +
                                    "Example: patron:add \"Gold Tier\" 123456789 10 JobIconCaptain true true true true true true";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                shell.WriteError(Help);
                return;
            }

            var name = args[0];
            if (string.IsNullOrWhiteSpace(name))
            {
                shell.WriteError("Tier name cannot be empty.");
                return;
            }

            if (!ulong.TryParse(args[1], out var discordRole))
            {
                shell.WriteError($"'{args[1]}' is not a valid Discord role id.");
                return;
            }

            var tiers = await Db.GetPatronTiers();
            if (tiers.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                shell.WriteError($"A patron tier named '{name}' already exists. Use 'patron:modify' to change it.");
                return;
            }

            if (tiers.Any(t => t.DiscordRole == discordRole))
            {
                shell.WriteError($"A patron tier with Discord role '{discordRole}' already exists.");
                return;
            }

            var priority = args.Length > 2 && int.TryParse(args[2], out var p) ? p : 0;
            var icon = args.Length > 3 && !string.IsNullOrEmpty(args[3]) && args[3] != "none" ? args[3] : null;

            var tier = new RMCPatronTier
            {
                Name = name,
                DiscordRole = discordRole,
                Priority = priority,
                Icon = icon,
                ShowOnCredits = ParseBool(args, 4),
                GhostColor = ParseBool(args, 5),
                GhostCosmetics = ParseBool(args, 6),
                GhostParticles = ParseBool(args, 7),
                LobbyMessage = ParseBool(args, 8),
                RoundEndShoutout = ParseBool(args, 9),
            };

            var id = await Db.AddPatronTier(tier);
            await LinkAccount.RefreshAllPatrons();
            LinkAccount.SendPatronsToAll();

            shell.WriteLine($"Patron tier '{name}' created with id {id}:");
            shell.WriteLine($"  Discord Role: {discordRole} | Priority: {priority} | Icon: {icon ?? "None"}");
            shell.WriteLine($"  Credits: {tier.ShowOnCredits} | Ghost Color: {tier.GhostColor}");
            shell.WriteLine($"  Ghost Cosmetics: {tier.GhostCosmetics} | Ghost Particles: {tier.GhostParticles}");
            shell.WriteLine($"  Lobby Message: {tier.LobbyMessage} | Shoutout: {tier.RoundEndShoutout}");
        }
        catch (Exception e)
        {
            shell.WriteError($"Error creating patron tier:\n{e}");
        }
    }

    private static bool ParseBool(string[] args, int index)
    {
        return args.Length > index && bool.TryParse(args[index], out var value) && value;
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint("<name>"),
            2 => CompletionResult.FromHint("<discordRole>"),
            3 => CompletionResult.FromHint("<priority?>"),
            4 => CompletionResult.FromHint("<icon?|none>"),
            5 => CompletionResult.FromHintOptions(BoolOptions, "<credits?>"),
            6 => CompletionResult.FromHintOptions(BoolOptions, "<ghostcolor?>"),
            7 => CompletionResult.FromHintOptions(BoolOptions, "<ghostcosmetics?>"),
            8 => CompletionResult.FromHintOptions(BoolOptions, "<ghostparticles?>"),
            9 => CompletionResult.FromHintOptions(BoolOptions, "<lobbymessage?>"),
            10 => CompletionResult.FromHintOptions(BoolOptions, "<shoutout?>"),
            _ => CompletionResult.Empty
        };
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronModifyCommand : BasePatronDbCommand
{
    private static readonly string[] Fields =
    [
        "name",
        "discordrole",
        "priority",
        "icon",
        "credits",
        "ghostcolor",
        "ghostcosmetics",
        "ghostparticles",
        "lobbymessage",
        "shoutout",
    ];

    private static readonly string[] BoolFields =
    [
        "credits",
        "ghostcolor",
        "ghostcosmetics",
        "ghostparticles",
        "lobbymessage",
        "shoutout",
    ];

    public override string Command => "patron:modify";

    public override string Description => "Modify a patron tier in the database";

    public override string Help => $"Usage: patron:modify <tierIdOrName> <field> <value>\n" +
                                    $"Fields: {string.Join(", ", Fields)}\n" +
                                    "Example: patron:modify \"Gold Tier\" ghostcosmetics true\n" +
                                    "Example: patron:modify \"Gold Tier\" icon none";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length != 3)
            {
                shell.WriteError(Help);
                return;
            }

            if (await FindTier(shell, args[0]) is not { } tier)
                return;

            var field = args[1].ToLowerInvariant();
            var value = args[2];

            switch (field)
            {
                case "name":
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        shell.WriteError("Tier name cannot be empty.");
                        return;
                    }

                    tier.Name = value;
                    break;
                case "discordrole":
                    if (!ulong.TryParse(value, out var role))
                    {
                        shell.WriteError($"'{value}' is not a valid Discord role id.");
                        return;
                    }

                    var tiers = await Db.GetPatronTiers();
                    if (tiers.Any(t => t.Id != tier.Id && t.DiscordRole == role))
                    {
                        shell.WriteError($"A patron tier with Discord role '{role}' already exists.");
                        return;
                    }

                    tier.DiscordRole = role;
                    break;
                case "priority":
                    if (!int.TryParse(value, out var priority))
                    {
                        shell.WriteError($"'{value}' is not a valid priority.");
                        return;
                    }

                    tier.Priority = priority;
                    break;
                case "icon":
                    tier.Icon = value is "none" or "clear" or "" ? null : value;
                    break;
                case "credits" or "ghostcolor" or "ghostcosmetics" or "ghostparticles" or "lobbymessage" or "shoutout":
                    if (!bool.TryParse(value, out var flag))
                    {
                        shell.WriteError($"'{value}' is not a valid boolean.");
                        return;
                    }

                    switch (field)
                    {
                        case "credits":
                            tier.ShowOnCredits = flag;
                            break;
                        case "ghostcolor":
                            tier.GhostColor = flag;
                            break;
                        case "ghostcosmetics":
                            tier.GhostCosmetics = flag;
                            break;
                        case "ghostparticles":
                            tier.GhostParticles = flag;
                            break;
                        case "lobbymessage":
                            tier.LobbyMessage = flag;
                            break;
                        case "shoutout":
                            tier.RoundEndShoutout = flag;
                            break;
                    }

                    break;
                default:
                    shell.WriteError($"Unknown field '{args[1]}'. Fields: {string.Join(", ", Fields)}");
                    return;
            }

            if (!await Db.UpdatePatronTier(tier))
            {
                shell.WriteError($"Patron tier '{tier.Name}' no longer exists.");
                return;
            }

            await RefreshPatrons(await GetTierMemberIds(tier.Id));
            shell.WriteLine($"Patron tier '{tier.Name}' updated: {field} = {value}");
        }
        catch (Exception e)
        {
            shell.WriteError($"Error modifying patron tier:\n{e}");
        }
    }

    public override async ValueTask<CompletionResult> GetCompletionAsync(
        IConsoleShell shell,
        string[] args,
        string argStr,
        CancellationToken cancel)
    {
        return args.Length switch
        {
            1 => await CompleteTierNames("<tierIdOrName>"),
            2 => CompletionResult.FromHintOptions(Fields, "<field>"),
            3 when BoolFields.Contains(args[1].ToLowerInvariant()) =>
                CompletionResult.FromHintOptions(BoolOptions, "<value>"),
            3 => CompletionResult.FromHint("<value>"),
            _ => CompletionResult.Empty
        };
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronDeleteCommand : BasePatronDbCommand
{
    public override string Command => "patron:delete";

    public override string Description => "Delete a patron tier from the database";

    public override string Help => "Usage: patron:delete <tierIdOrName> [force]\n" +
                                    "Deleting a tier with 'force' also removes all patrons in it.\n" +
                                    "Example: patron:delete \"Gold Tier\"";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            if (args.Length is < 1 or > 2)
            {
                shell.WriteError(Help);
                return;
            }

            if (await FindTier(shell, args[0]) is not { } tier)
                return;

            var members = await GetTierMemberIds(tier.Id);
            var force = args.Length == 2 && args[1].Equals("force", StringComparison.OrdinalIgnoreCase);
            if (members.Count > 0 && !force)
            {
                shell.WriteError($"Patron tier '{tier.Name}' has {members.Count} patron(s). " +
                                 "Move them with 'patron:set' first, or append 'force' to delete them along with the tier.");
                return;
            }

            if (!await Db.DeletePatronTier(tier.Id))
            {
                shell.WriteError($"Patron tier '{tier.Name}' no longer exists.");
                return;
            }

            await RefreshPatrons(members);
            shell.WriteLine(members.Count > 0
                ? $"Deleted patron tier '{tier.Name}' and removed {members.Count} patron(s) from it."
                : $"Deleted patron tier '{tier.Name}'.");
        }
        catch (Exception e)
        {
            shell.WriteError($"Error deleting patron tier:\n{e}");
        }
    }

    public override async ValueTask<CompletionResult> GetCompletionAsync(
        IConsoleShell shell,
        string[] args,
        string argStr,
        CancellationToken cancel)
    {
        return args.Length switch
        {
            1 => await CompleteTierNames("<tierIdOrName>"),
            2 => CompletionResult.FromHintOptions(["force"], "[force]"),
            _ => CompletionResult.Empty
        };
    }
}

[AdminCommand(AdminFlags.Host)]
internal sealed class PatronSetCommand : BasePatronDbCommand
{
    public override string Command => "patron:set";

    public override string Description => "Set or remove a player's patron tier in the database";

    public override string Help => "Usage: patron:set <username> <tierIdOrName|none>\n" +
                                    "Example: patron:set \"John Doe\" \"Gold Tier\"\n" +
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

            if (await FindTier(shell, args[1]) is not { } tier)
                return;

            await Db.SetPatron(player.UserId, tier.Id);
            await RefreshPatrons([player.UserId.UserId]);
            shell.WriteLine($"Set '{player.LastSeenUserName}' to patron tier '{tier.Name}'.");
        }
        catch (Exception e)
        {
            shell.WriteError($"Error setting patron:\n{e}");
        }
    }

    public override async ValueTask<CompletionResult> GetCompletionAsync(
        IConsoleShell shell,
        string[] args,
        string argStr,
        CancellationToken cancel)
    {
        if (args.Length == 1)
        {
            var playerNames = PlayerManager.Sessions.Select(s => s.Name);
            return CompletionResult.FromHintOptions(playerNames, "<username>");
        }

        if (args.Length == 2)
        {
            var tiers = await Db.GetPatronTiers();
            return CompletionResult.FromHintOptions(tiers.Select(t => t.Name).Append("none"), "<tierIdOrName|none>");
        }

        return CompletionResult.Empty;
    }
}
