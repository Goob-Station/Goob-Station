using System.Linq;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Heretic.EntitySystems;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Shitcode.Heretic.Commands;

/// <summary>
/// Debug command to instantly ascend as a heretic,
/// mainly for testing the ascension cinematics.
/// </summary>
[AdminCommand(AdminFlags.Debug)]
public sealed class HereticAscendCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;

    public override string Command => "hereticascend";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player || player.AttachedEntity is not { } body)
        {
            shell.WriteError(Loc.GetString("cmd-hereticascend-no-entity"));
            return;
        }

        // Extreme cause it affects everyones screen ingame
        _adminLog.Add(LogType.AdminCommands, LogImpact.Extreme, $"{EntityManager.ToPrettyString(body):player} used {Command}");


        if (!_heretic.TryGetHereticComponent(body, out var heretic, out var mind))
        {
            _antag.ForceMakeAntag<HereticRuleComponent>(player, "Heretic");

            if (!_heretic.TryGetHereticComponent(body, out heretic, out mind))
            {
                shell.WriteError(Loc.GetString("cmd-hereticascend-antag-failed"));
                return;
            }
        }

        var path = args.Length > 0 ? args[0] : heretic.CurrentPath ?? "Ash";
        var ascension = _proto.EnumeratePrototypes<HereticKnowledgePrototype>()
            .FirstOrDefault(k => k.Stage == 10
                && !k.SideKnowledge
                && string.Equals(k.Path, path, StringComparison.OrdinalIgnoreCase));

        if (ascension?.Path is not { } properPath)
        {
            shell.WriteError(Loc.GetString("cmd-hereticascend-bad-path", ("path", path)));
            return;
        }

        heretic.CurrentPath = properPath;
        heretic.CanAscend = true;

        if (heretic.Ascended)
            heretic.Ascended = false;
        else
            _heretic.TryAddKnowledge((mind, heretic), ascension.ID, body);

        EntityManager.EventBus.RaiseLocalEvent(mind, (object) new EventHereticAscension(), true);


        shell.WriteLine(Loc.GetString("cmd-hereticascend-success", ("path", properPath)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        var paths = _proto.EnumeratePrototypes<HereticKnowledgePrototype>()
            .Where(k => k is { Stage: 10, SideKnowledge: false, Path: not null })
            .Select(k => k.Path!)
            .Distinct()
            .Order();

        return CompletionResult.FromHintOptions(paths, Loc.GetString("cmd-hereticascend-path-hint"));
    }
}
