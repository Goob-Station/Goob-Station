using System.Linq;
using System.Text;
using Content.Server.Administration;
using Content.Server.Mind;
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

        if (!_entity.System<MindSystem>().TryGetMind(entity.Value, out _, out var mind))
        {
            shell.WriteLine("");
            return;
        }

        StringBuilder builder = new();

        builder.AppendJoin('\n', mind.Skills.Order());

        builder.Append('\n');

        shell.WriteLine(builder.ToString());
    }
}
