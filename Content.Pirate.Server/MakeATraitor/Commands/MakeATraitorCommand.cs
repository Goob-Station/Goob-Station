using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Shared.Administration;
using Content.Shared.Ghost;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using System;

namespace Content.Server._Pirate.MakeATraitor.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class MakeATraitorCommand : IConsoleCommand
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IEntityManager EntityManager = default!;

        // [Dependency] private readonly ThiefRuleSystem _thief = default!;
        // [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
        // [Dependency] private readonly RevolutionaryRuleSystem _revolutionaryRule = default!;

        public string Command => "maketraitor";

        public string Description => "робить вибраного гравця зрадником";

        public string Help => "maketraitor <uid> <type>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player != null)
            {
                shell.WriteLine("You cannot use this command from the player.");
                return;
            }

            if (args.Length != 2)
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

            if (!Enum.TryParse<MakeATraitorSystem.TraitorType>(args[1], true, out var traitorType))
            {
                shell.WriteLine("invalid traitor type");
                return;
            }

            if (EntityManager.HasComponent<GhostComponent>(target))
            {
                shell.WriteLine("Ghost cannot be made a traitor");
                return;
            }

            EntityManager.System<MakeATraitorSystem>().MakeTraitor(traitorType, target.Value);
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                2 => CompletionResult.FromHintOptions(Enum.GetNames<MakeATraitorSystem.TraitorType>(),
                    "Тип зрадника"),
                _ => CompletionResult.Empty
            };
        }
    }
}
