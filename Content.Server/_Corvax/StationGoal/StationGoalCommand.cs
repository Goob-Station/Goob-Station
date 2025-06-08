using System.Linq;
using Content.Server.Administration;
using Content.Server.Commands;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Corvax.StationGoal
{
    [AdminCommand(AdminFlags.Fun)]
    public sealed class StationGoalCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public string Command => "sendstationgoal";
        public string Description => Loc.GetString("send-station-goal-command-description");
        public string Help => Loc.GetString("send-station-goal-command-help-text", ("command", Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!NetEntity.TryParse(args[0], out var euidNet) || !_entManager.TryGetEntity(euidNet, out var euid))
            {
                shell.WriteError($"Failed to parse euid '{args[0]}'.");
                return;
            }

            var protoId = args[1];
            if (!_prototypeManager.TryIndex<StationGoalPrototype>(protoId, out var proto))
            {
                shell.WriteError($"No station goal found with ID {protoId}!");
                return;
            }

            var stationGoalPaper = _entManager.System<StationGoalPaperSystem>();
            if (!stationGoalPaper.SendStationGoal(euid.Value, protoId))
            {
                shell.WriteError("Station goal was not sent");
                return;
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    var stations = ContentCompletionHelper.StationIds(_entManager);
                    return CompletionResult.FromHintOptions(stations, Loc.GetString("send-station-goal-command-arg-station"));
                case 2:
                    var options = _prototypeManager
                        .EnumeratePrototypes<StationGoalPrototype>()
                        .Select(p => new CompletionOption(p.ID));

                    return CompletionResult.FromHintOptions(options, Loc.GetString("send-station-goal-command-arg-id"));
            }
            return CompletionResult.Empty;
        }
    }
}
