using Content.Server.Administration;
using Content.Server.Cargo.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.IoC;
using Robust.Shared.GameObjects;

namespace Content.Server._ShibaStation.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class RemoveGridsByValueCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public string Command => "rmgridsbyvalue";
    public string Description => "Removes all grids with a value less than or equal to the specified threshold (default: 0).";
    public string Help => "rmgridsbyvalue [maxValue]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var maxValue = 0.0;
        if (args.Length > 0 && !double.TryParse(args[0], out maxValue))
        {
            shell.WriteError("Invalid maxValue argument. Please provide a valid number.");
            return;
        }

        var pricingSystem = _entManager.System<PricingSystem>();
        var mapManager = IoCManager.Resolve<IMapManager>();
        var player = shell.Player;

        // Get the player's current map if they're in-game
        MapId targetMap;
        if (player?.AttachedEntity != null)
        {
            var transform = _entManager.GetComponent<TransformComponent>(player.AttachedEntity.Value);
            targetMap = transform.MapID;
        }
        else if (args.Length > 1 && int.TryParse(args[1], out var mapId))
        {
            targetMap = new MapId(mapId);
        }
        else
        {
            shell.WriteError("No valid map found. Either use this command in-game or specify a map ID.");
            return;
        }

        if (!mapManager.MapExists(targetMap))
        {
            shell.WriteError($"Map {targetMap} does not exist.");
            return;
        }

        var gridsToDelete = new List<EntityUid>();
        var gridsChecked = 0;

        // Find all grids on the map
        foreach (var grid in mapManager.GetAllGrids(targetMap))
        {
            if (!_entManager.HasComponent<MapGridComponent>(grid.Owner))
                continue;

            gridsChecked++;
            var gridValue = pricingSystem.AppraiseGrid(grid.Owner);

            if (gridValue <= maxValue)
                gridsToDelete.Add(grid.Owner);
        }

        // Delete the identified grids
        foreach (var gridUid in gridsToDelete)
        {
            _entManager.DeleteEntity(gridUid);
        }

        shell.WriteLine($"Checked {gridsChecked} grids. Removed {gridsToDelete.Count} grids with value <= {maxValue}.");
    }
}
