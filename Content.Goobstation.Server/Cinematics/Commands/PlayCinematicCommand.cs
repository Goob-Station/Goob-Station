using System.Linq;
using Content.Goobstation.Shared.Cinematic;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Shared.Administration;
using Content.Shared.Database;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cinematics.Commands;

/// <summary>
/// Plays a scripted cinematic on an entity.
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed class PlayCinematicCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedCinematicSystem _cinematic = default!;

    public override string Command => "playcinematic";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        EntityUid target;

        switch (args.Length)
        {
            case 1:
                if (player?.AttachedEntity is not { } playerEntity)
                {
                    shell.WriteError(Loc.GetString("cmd-playcinematic-no-entity"));
                    return;
                }

                target = playerEntity;
                break;
            case 2:
                if (!_playerManager.TryGetSessionByUsername(args[1], out var session))
                {
                    shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
                    return;
                }

                if (session.AttachedEntity is not { } targetEntity)
                {
                    shell.WriteError(Loc.GetString("cmd-playcinematic-target-no-entity", ("username", args[1])));
                    return;
                }

                target = targetEntity;
                break;
            default:
                shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 1), ("upper", 2)));
                return;
        }

        var cinematic = args[0];

        if (!_proto.HasIndex<CinematicPrototype>(cinematic))
        {
            shell.WriteError(Loc.GetString("cmd-playcinematic-invalid-prototype", ("cinematic", cinematic)));
            return;
        }

        var targetName = EntityManager.ToPrettyString(target);
        var subject = EntityManager.GetComponent<MetaDataComponent>(target).EntityName;

        if (!_cinematic.TryStartCinematic(target, cinematic, subject))
            return;

        _adminLog.Add(LogType.AdminCommands,
            LogImpact.Medium,
            $"{player?.Name ?? "Server"} played cinematic {cinematic} on {targetName:target}");

        shell.WriteLine(Loc.GetString("cmd-playcinematic-success",
            ("cinematic", cinematic),
            ("target", targetName)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                _proto.EnumeratePrototypes<CinematicPrototype>().Select(p => p.ID).Order(),
                Loc.GetString("cmd-playcinematic-cinematic-hint")),
            2 => CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("shell-argument-username-optional-hint")),
            _ => CompletionResult.Empty,
        };
    }
}
