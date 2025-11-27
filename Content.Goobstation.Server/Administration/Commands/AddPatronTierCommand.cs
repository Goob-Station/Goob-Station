using Content.Server._RMC14.LinkAccount;
using Content.Server.Administration;
using Content.Shared._RMC14.LinkAccount;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Host)]
internal sealed class AddPatronTierCommand : LocalizedCommands
{
    private static readonly string[] BoolOptions = ["true", "false"];

    [Dependency] private readonly LinkAccountManager _linkAccount = default!;

    public override string Command => "addpatrontier";

    public override string Description => "Create a debug patron tier for testing";

    public override string Help => "Usage: addpatrontier <tierId> <tierName> <icon?> <credits?> <ghostcolor?> <lobbymessage?> <shoutout?>\n" +
                                    "Example: addpatrontier captain Captain JobIconCaptain true true true true";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Usage: addpatrontier <tierId> <tierName> <icon?> <credits?> <ghostcolor?> <lobbymessage?> <shoutout?>");
            shell.WriteError("Example: addpatrontier captain \"Captain\" JobIconCaptain true true true true");
            return;
        }

        var tierId = args[0];
        var tierName = args[1];
        var icon = args.Length > 2 && !string.IsNullOrEmpty(args[2]) ? args[2] : null;
        var showOnCredits = args.Length > 3 && bool.TryParse(args[3], out var credits) && credits;
        var ghostColor = args.Length > 4 && bool.TryParse(args[4], out var ghost) && ghost;
        var lobbyMessage = args.Length > 5 && bool.TryParse(args[5], out var lobby) && lobby;
        var roundEndShoutout = args.Length > 6 && bool.TryParse(args[6], out var shoutout) && shoutout;

        var tier = new SharedRMCPatronTier(
            ShowOnCredits: showOnCredits,
            GhostColor: ghostColor,
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
            6 => CompletionResult.FromHintOptions(BoolOptions, "<lobbymessage?>"),
            7 => CompletionResult.FromHintOptions(BoolOptions, "<shoutout?>"),
            _ => CompletionResult.Empty
        };
    }
}
