using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class LavalandListingCommand : IConsoleCommand
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "listlavaland";

    public string Description => "Logs a list of all active lavaland maps into the console.";

    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var lavalands = _entityManager.System<LavalandPlanetSystem>().GetLavalands();

        foreach (var (owner, comp) in lavalands)
        {
            var lavalandString = $"Type: {comp.PrototypeId} , MapID: {comp.MapId} , MapUid: {owner} , Seed: {comp.Seed}";
            shell.WriteLine(lavalandString);
        }
    }
}
