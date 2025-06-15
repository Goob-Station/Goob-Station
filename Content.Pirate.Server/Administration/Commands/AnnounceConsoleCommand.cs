using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Pirate.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AnnounceConsoleCommand : IConsoleCommand
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        public string Command => "announce-console";
        public string Description => "Відправляє оголошення OOC з консолі";
        public string Help => $"{Command} <message> to send announcement as console.";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length == 0)
            {
                shell.WriteError("Not enough arguments! Need at least 1.");
                return;
            }

            var message = string.Join(" ", args);
            _chatManager.DispatchServerAnnouncement(message, Color.Orange);
            shell.WriteLine("Sent!");
        }
    }
}
