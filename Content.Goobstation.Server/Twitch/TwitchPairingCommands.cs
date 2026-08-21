using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using System.Linq;

namespace Content.Goobstation.Server.Twitch;

[AdminCommand(AdminFlags.Admin)]
public sealed class TwitchLinkCommand : LocalizedCommands
{
    public override string Command => "twitchlink";
    public override string Description => "Links a Twitch broadcaster pairing code to an SS14 account.";
    public override string Help => "Usage: twitchlink <code> <SS14 username>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Help);
            return;
        }

        var system = IoCManager.Resolve<IEntityManager>().System<TwitchPairingSystem>();
        if (!system.TryComplete(args[0], string.Join(' ', args[1..]), out var pairing, out var error))
        {
            shell.WriteError(error);
            return;
        }

        shell.WriteLine($"Linked Twitch channel {pairing!.ChannelLogin} ({pairing.ChannelId}) to {pairing.Ss14Username}.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length >= 2
            ? CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<SS14 username>")
            : CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class TwitchUnlinkCommand : LocalizedCommands
{
    public override string Command => "twitchunlink";
    public override string Description => "Removes a Twitch broadcaster pairing.";
    public override string Help => "Usage: twitchunlink <channel ID or login>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        var system = IoCManager.Resolve<IEntityManager>().System<TwitchPairingSystem>();
        if (!system.TryUnlink(args[0], out var pairing))
        {
            shell.WriteError("No Twitch pairing matched that channel ID or login.");
            return;
        }

        shell.WriteLine($"Unlinked Twitch channel {pairing!.ChannelLogin} from {pairing.Ss14Username}.");
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class TwitchLinksCommand : LocalizedCommands
{
    public override string Command => "twitchlinks";
    public override string Description => "Lists Twitch broadcaster pairings.";
    public override string Help => "Usage: twitchlinks";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 0)
        {
            shell.WriteError(Help);
            return;
        }

        var pairings = IoCManager.Resolve<IEntityManager>()
            .System<TwitchPairingSystem>()
            .Pairings
            .OrderBy(pairing => pairing.ChannelLogin, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (pairings.Length == 0)
        {
            shell.WriteLine("No Twitch channels are linked.");
            return;
        }

        foreach (var pairing in pairings)
            shell.WriteLine($"{pairing.ChannelLogin} ({pairing.ChannelId}) -> {pairing.Ss14Username} ({pairing.Ss14UserId})");
    }
}
