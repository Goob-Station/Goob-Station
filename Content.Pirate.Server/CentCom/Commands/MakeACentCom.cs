using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Pirate.Server.CentCom.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class MakeACentCom : IConsoleCommand
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IEntityManager EntityManager = default!;

        // [Dependency] private readonly ThiefRuleSystem _thief = default!;
        // [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
        // [Dependency] private readonly RevolutionaryRuleSystem _revolutionaryRule = default!;

        public string Command => "makecentcom";

        public string Description => "робить вибраного гравця працівником ЦентКому";

        public string Help => "makecentcom <uid>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player != null)
            {
                shell.WriteLine("You cannot use this command from the player.");
                return;
            }

            if (args.Length != 1)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!int.TryParse(args[0], out var intUid))
            {
                shell.WriteError("uid must be a number");
                return;
            }

            var targetNet = new NetEntity(intUid);

            if (!EntityManager.TryGetEntity(targetNet, out var target))
            {
                shell.WriteError("cannot find entity");
                return;
            }

            EntityManager.System<MakeACentComSystem>().MakeAnOfficial(target.Value);
        }
    }
}
