using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Administration;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Content.Server.Administration;

namespace Content.Server._Shitmed.Medical.Trauma;

[AdminCommand(AdminFlags.Debug)]
public sealed class AddTraumaCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public string Command => "addtrauma";
    public string Description => "Inflicts a trauma on a body, on a specific part type if given.";
    public string Help => "Usage: addtrauma <target> <traumaType> [partType] [severity]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 2 or > 4)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet) || !_entManager.TryGetEntity(targetNet, out var target))
        {
            shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!_proto.HasIndex<TraumaTypePrototype>(args[1]))
        {
            shell.WriteError($"No trauma type prototype '{args[1]}'. Known: {string.Join(", ", _proto.EnumeratePrototypes<TraumaTypePrototype>().Select(p => p.ID))}.");
            return;
        }
        var traumaType = new ProtoId<TraumaTypePrototype>(args[1]);

        BodyPartType? partFilter = null;
        if (args.Length >= 3)
        {
            if (!Enum.TryParse<BodyPartType>(args[2], true, out var pt))
            {
                shell.WriteError($"Unknown body part type '{args[2]}'. Valid: {string.Join(", ", Enum.GetNames<BodyPartType>())}.");
                return;
            }
            partFilter = pt;
        }

        var severity = FixedPoint2.New(12);
        if (args.Length >= 4)
        {
            if (!float.TryParse(args[3], out var sev))
            {
                shell.WriteError($"Severity '{args[3]}' is not a number.");
                return;
            }
            severity = FixedPoint2.New(sev);
        }

        var body = _entManager.System<SharedBodySystem>();
        var trauma = _entManager.System<TraumaSystem>();

        Entity<WoundableComponent>? chosen = null;
        foreach (var (partId, partComp) in body.GetBodyChildren(target.Value))
        {
            if (!_entManager.TryGetComponent<WoundableComponent>(partId, out var woundable)
                || (partFilter != null && partComp.PartType != partFilter))
                continue;

            chosen = (partId, woundable);
            break;
        }

        if (chosen == null)
        {
            shell.WriteError(partFilter != null
                ? $"No woundable {partFilter} part found on {_entManager.ToPrettyString(target.Value)}."
                : $"No woundable parts found on {_entManager.ToPrettyString(target.Value)}.");
            return;
        }

        if (trauma.TryInflictTrauma(chosen.Value, traumaType, severity))
            shell.WriteLine($"Inflicted {traumaType} on {_entManager.ToPrettyString(chosen.Value.Owner)}.");
        else
            shell.WriteError($"Failed to inflict {traumaType}. The part must be attached to a living body.");
    }
}
