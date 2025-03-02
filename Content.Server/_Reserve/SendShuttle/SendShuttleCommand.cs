using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server._Reserve.ERT.SendShuttleSystem;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared._Reserve.ERT.SendShuttlePrototype;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server_Reserve.ERT.SendShuttleCommand;

[AdminCommand(AdminFlags.Spawn)]
public sealed class SendShuttleCommand : IConsoleCommand
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IEntitySystemManager _system = default!;

    public string Command => "sendshuttle";
    public string Description => Loc.GetString("send-shuttle-description");
    public string Help => Loc.GetString("send-shuttle-help");
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        bool playAnnounce;
        var player = shell.Player;

        if (player?.AttachedEntity == null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        switch (args.Length)
        {
            case 0:
                shell.WriteLine(Loc.GetString("send-shuttle-help"));
                return;

            case 1:
                playAnnounce = true;
                break;

            default:
                if (bool.TryParse(args[1].ToLower(), out var temp))
                {
                    playAnnounce = temp;
                }
                else
                {
                    shell.WriteError(Loc.GetString($"send-shuttle-truefalse-error"));
                    return;
                }
                break;
        }

        _system.GetEntitySystem<SendShuttle>().SpawnShuttle(args[0], playAnnounce);

        _adminLogger.Add(LogType.Action, LogImpact.High, $"{player} отправил ОБР. Тип: {args[0]}");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = IoCManager.Resolve<IPrototypeManager>()
                .EnumeratePrototypes<SendShuttlePrototype>()
                .Select(proto => new CompletionOption(proto.ID, Loc.GetString(proto.HintText)));
            return CompletionResult.FromHintOptions(options, Loc.GetString("send-shuttle-hint-type"));
        }

        if (args.Length == 2)
        {
            var options = new CompletionOption[]
            {
                new("true", Loc.GetString("send-shuttle-hint-isannounce-true")),
                new("false", Loc.GetString("send-shuttle-hint-isannounce-false")),
            };
            return CompletionResult.FromHintOptions(options, Loc.GetString("send-shuttle-hint-isannounce"));
        }

        return CompletionResult.Empty;
    }
}
