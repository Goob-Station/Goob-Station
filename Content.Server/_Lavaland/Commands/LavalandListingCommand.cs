using Content.Server._Lavaland.Procedural.Prototypes;
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

    public string Command => "lavaland_list";

    public string Description => "Generates a lavaland world if it doesn't exist. Be careful, this can cause freezes on runtime!";

    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var lavalands = _entityManager.System<LavalandGenerationSystem>().LavalandMaps;
        foreach (var lavaland in lavalands)
        {
            shell.WriteLine(lavaland.ToString());
        }
    }
}
