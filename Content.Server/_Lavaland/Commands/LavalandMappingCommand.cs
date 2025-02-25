using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Administration;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Commands;

[AdminCommand(AdminFlags.Mapping)]
public sealed class LavalandMappingCommand : IConsoleCommand
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "mappinglavaland";

    public string Description => "Loads lavaland world on a new map. Be careful, this can cause freezes on runtime!";

    public string Help => "mappinglavaland <prototype id> <seed>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        LavalandMapPrototype? lavalandProto;
        int? lavalandSeed = null;

        switch (args.Length)
        {
            case 1:
                if (!_proto.TryIndex(args[0], out lavalandProto))
                {
                    shell.WriteLine(Loc.GetString("Wrong lavaland prototype!"));
                    return;
                }
                break;
            case 2:
                if (!_proto.TryIndex(args[0], out lavalandProto))
                {
                    shell.WriteLine(Loc.GetString("Wrong lavaland prototype!"));
                    return;
                }

                if (!ushort.TryParse(args[1], out var targetId))
                {
                    shell.WriteLine(Loc.GetString("shell-argument-must-be-number"));
                    return;
                }
                lavalandSeed = targetId;
                break;
            default:
                shell.WriteLine(Loc.GetString("cmd-playerpanel-invalid-arguments"));
                shell.WriteLine(Help);
                return;
        }

        if (!_entityManager.System<LavalandPlanetSystem>().SetupLavalandPlanet(out var lavaland, lavalandProto, lavalandSeed))
            shell.WriteLine("Failed to load lavaland due to an exception!");

        shell.WriteLine($"Successfully created new lavaland map: {_entityManager.ToPrettyString(lavaland)}");
    }
}
