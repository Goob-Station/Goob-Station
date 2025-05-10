using Robust.Shared.Console;

namespace Content.Casino.Client.Games;

public sealed class SlotCommand : IConsoleCommand
{
    public string Command => "slots";
    public string Description => "Play the slot machine";
    public string Help => "slots <bet>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Usage: slots <bet>");
            return;
        }

        if (!int.TryParse(args[0], out var bet) || bet <= 0)
        {
            shell.WriteError("Bet must be a positive number");
            return;
        }

        var player = shell.Player;
        if (player == null)
        {
            shell.WriteError("You must be in-game to use this command");
            return;
        }

        var casino = IoCManager.Resolve<ClientGameManager>();
        casino.GetClient<SlotsClient>().Play(bet);
    }
}
