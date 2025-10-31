using System.Linq;
using Content.Server.Administration;
using Content.Server.Mind;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Skills.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class RevokeSkillCommand : IConsoleCommand
{
    [Dependency] private readonly ILocalizationManager _localization = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public string Command => "revokeskill";

    public string Description => "Revokes skill from given entity.";

    public string Help => "revokeskill <entityuid> <skill>";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(_localization.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var id))
        {
            shell.WriteError(_localization.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!_entity.TryGetEntity(id, out var entity))
        {
            shell.WriteError(_localization.GetString("shell-invalid-entity-id"));
            return;
        }

        if (!_entity.System<MindSystem>().TryGetMind(entity.Value, out _, out var mind))
        {
            shell.WriteError(_localization.GetString("shell-invalid-entity-id"));
            return;
        }

        if (!Enum.TryParse<Shared._CorvaxGoob.Skills.Skills>(args[1], out var skill))
        {
            shell.WriteError("No such skill.");
            return;
        }

        mind.Skills.Remove(skill);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 2)
        {
            var component = int.TryParse(args[0], out var id)
                ? _entity.TryGetEntity(new(id), out var entity)
                    ? _entity.System<MindSystem>().TryGetMind(entity.Value, out _, out var comp)
                        ? comp
                        : null
                    : null
                : null;

            return CompletionResult.FromOptions(Enum.GetValues<Shared._CorvaxGoob.Skills.Skills>()
                .Where(value => component?.Skills.Contains(value) != false)
                .Select(value => value.ToString())
                .Where(name => name.ToString().StartsWith(args[1], true, null))
                .Select(value => new CompletionOption(value.ToString())));
        }

        return CompletionResult.Empty;
    }
}
