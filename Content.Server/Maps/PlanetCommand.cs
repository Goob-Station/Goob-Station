using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Server.Parallax;
using Content.Shared.Administration;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural.Loot;
using Content.Shared.Random;
using Content.Shared.Salvage;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Maps;

/// <summary>
/// Converts the supplied map into a "planet" with defaults.
/// </summary>
[AdminCommand(AdminFlags.Mapping)]
public sealed class PlanetCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;



    public string Command => "planet";
    public string Description => Loc.GetString("cmd-planet-desc");
    public string Help => Loc.GetString("cmd-planet-help", ("command", Command));
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString($"cmd-planet-args"));
            return;
        }

        if (!int.TryParse(args[0], out var mapInt))
        {
            shell.WriteError(Loc.GetString($"cmd-planet-map", ("map", mapInt)));
            return;
        }

        var mapId = new MapId(mapInt);

        if (!_mapManager.MapExists(mapId))
        {
            shell.WriteError(Loc.GetString($"cmd-planet-map", ("map", mapId)));
            return;
        }

        if (!_protoManager.TryIndex<BiomeTemplatePrototype>(args[1], out var biomeTemplate))
        {
            shell.WriteError(Loc.GetString("cmd-planet-map-prototype", ("prototype", args[1])));
            return;
        }

        var biomeSystem = _entManager.System<BiomeSystem>();
        var mapUid = _mapManager.GetMapEntityId(mapId);
        biomeSystem.EnsurePlanet(mapUid, biomeTemplate);

        // - Beginning of GoobStation changes -

        var budgetEntries = new List<IBudgetEntry>();
        var randomSystem = _entManager.System<RandomSystem>();

        foreach (var lootProto in _protoManager.EnumeratePrototypes<SalvageLootPrototype>())
        {
            if (!lootProto.Guaranteed)
                continue;

            await SpawnDungeonLoot(lootProto, mapUid);
        }
        var probSum = budgetEntries.Sum(x => x.Prob);
        var allLoot = _protoManager.Index<SalvageLootPrototype>(SharedSalvageSystem.ExpeditionsLootProto);
        var seed = _entManager.GetComponent<BiomeComponent>(mapUid).Seed;
        var random = new Random(seed);


        shell.WriteLine(Loc.GetString("cmd-planet-success", ("mapId", mapId)));
    }

    private async Task SpawnDungeonLoot(SalvageLootPrototype loot, EntityUid gridUid)
    {
        var biomeSystem = _entManager.System<BiomeSystem>();
        for (var i = 0; i < loot.LootRules.Count; i++)
        {
            var rule = loot.LootRules[i];

            switch (rule)
            {
                case BiomeMarkerLoot biomeLoot:
                    {
                        if (_entManager.TryGetComponent<BiomeComponent>(gridUid, out var biome))
                        {
                            biomeSystem.AddMarkerLayer(gridUid, biome, biomeLoot.Prototype);
                        }
                    }
                    break;
                case BiomeTemplateLoot biomeLoot:
                    {
                        if (_entManager.TryGetComponent<BiomeComponent>(gridUid, out var biome))
                        {
                            biomeSystem.AddTemplate(gridUid, biome, "Loot", _protoManager.Index<BiomeTemplatePrototype>(biomeLoot.Prototype), i);
                        }
                    }
                    break;
            }
        }
    }

    // - End of GoobStation changes -

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.MapIds(_entManager), "Map Id");

        if (args.Length == 2)
        {
            var options = _protoManager.EnumeratePrototypes<BiomeTemplatePrototype>()
                .Select(o => new CompletionOption(o.ID, "Biome"));
            return CompletionResult.FromOptions(options);
        }

        return CompletionResult.Empty;
    }
}
