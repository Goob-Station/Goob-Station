using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Skills.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class GrantAllSkillsCommand : IConsoleCommand
{
    [Dependency] private readonly ILocalizationManager _localization = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public string Command => "grantallskills";

    public string Description => "Grants all skills to given entity.";

    public string Help => "grantallskills <entityuid>";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (args.Length != 1)
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

        _entity.System<SkillsSystem>().GrantAllSkills(entity.Value);
    }
}
