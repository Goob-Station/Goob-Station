using System.Linq;
using System.Text;
using Content.Server.Administration;
using Content.Shared._CorvaxGoob.Skills;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Skills.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class ListSkillsCommand : IConsoleCommand
{
    [Dependency] private readonly ILocalizationManager _localization = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public string Command => "listskills";

    public string Description => "List skills of given entity.";

    public string Help => "listskills <entityuid>";

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

        if (!_entity.TryGetComponent<SkillsComponent>(entity.Value, out var component))
        {
            shell.WriteLine("");
            return;
        }

        StringBuilder builder = new();

        builder.AppendJoin('\n', component.Skills.Order());

        builder.Append('\n');

        shell.WriteLine(builder.ToString());
    }
}
