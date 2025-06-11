using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Pirate.Server.Pacification.Managers;
using Robust.Shared.Player;
using Robust.Shared.IoC;

namespace Content.Pirate.Server.Pacification.Commands
{
    [AdminCommand(AdminFlags.Moderator)]
    public sealed class PirateUnpacifyCommand : IConsoleCommand
    {
        public string Command => "pirate_unpacify";
        public string Description => "Видаляє гравця зі списку pacify.";
        public string Help => "Використання: pirate_unpacify <ім'я_гравця>";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length == 0)
            {
                shell.WriteError("Ви повинні вказати ім'я гравця.");
                return;
            }

            var loc = IoCManager.Resolve<IPlayerLocator>();
            var name = args[0].Trim();
            var data = await loc.LookupIdByNameAsync(name);

            if (data != null)
            {
                new PacifyManager().RemoveFromPacify(data.UserId.ToString());
                shell.WriteLine($"Гравець {data.Username} був видалений зі списку pacify.");
            }
            else
            {
                shell.WriteError($"Гравця з ім'ям {name} не знайдено.");
            }
        }
    }
}
