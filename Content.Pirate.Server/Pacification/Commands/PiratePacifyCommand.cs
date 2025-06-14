using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Pirate.Server.Pacification.Managers;
using Robust.Shared.Player;
using Robust.Shared.IoC;

namespace Content.Pirate.Server.Pacification.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class PiratePacifyCommand : IConsoleCommand
    {
        public string Command => "pirate_pacify";
        public string Description => "Додає гравця до списку pacify.";
        public string Help => "Використання: pirate_pacify <ім'я_гравця> <кількість_днів>";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 2)
            {
                shell.WriteError("Ви повинні вказати ім'я гравця і кількість днів.");
                return;
            }

            if (!int.TryParse(args[1], out var days))
            {
                shell.WriteError("Кількість днів повинна бути числом.");
                return;
            }

            if (days < 1)
            {
                shell.WriteError("Кількість днів повинна бути щонайменше 1.");
                return;
            }

            var loc = IoCManager.Resolve<IPlayerLocator>();
            var name = args[0].Trim();
            var data = await loc.LookupIdByNameAsync(name);

            if (data != null)
            {
                new PacifyManager().AddToPacify(data.UserId.ToString(), data.Username, days);
                shell.WriteLine($"Гравець {data.Username} був доданий до списку pacify на {days} днів.");
            }
            else
            {
                shell.WriteError($"Гравця з ім'ям {name} не знайдено.");
            }
        }
    }
}
