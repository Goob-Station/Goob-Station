using Content.Goobstation.Server.Cursed;
using Content.Goobstation.Shared.BlockSuicide;
using Content.Shared.Interaction.Components;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Administration.Systems;

/// <summary>
/// This handles loading the Hell map for the helldrag smite.
/// </summary>
public sealed class AdminHellSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;

    private const string HellMapPath = "/Maps/_Goobstation/Nonstations/hell.yml";

    private Dictionary<NetUserId, EntityUid> HellMap { get; set; } = new();
    private Dictionary<NetUserId, EntityUid?> HellGrid { get; set; } = new();

    public (EntityUid Map, EntityUid? Grid) AssertHellLoaded(EntityUid targetUserId, NetUserId targetNetId)
    {
        if (HellMap.TryGetValue(targetNetId , out var hellMap) && !Deleted(hellMap) && !Terminating(hellMap))
        {
            if (HellGrid.TryGetValue(targetNetId, out var hellGrid) && !Deleted(hellGrid) && !Terminating(hellGrid.Value))
            {
                return (hellMap, hellGrid);
            }

            HellGrid[targetNetId] = null;
            return (hellMap, null);
        }

        var path = new ResPath(HellMapPath);
        var mapUid = _maps.CreateMap(out var mapId);

        if (!_loader.TryLoadGrid(mapId, path, out var grid))
        {
            QueueDel(mapUid);
            throw new Exception($"Failed to load Hell. Ironic.");
        }

        TryName(targetUserId, out var name);

        HellMap[targetNetId] = mapUid;
        _metaDataSystem.SetEntityName(mapUid, $"HELLMAP-{name}");

        HellGrid[targetNetId] = grid.Value.Owner;
        _metaDataSystem.SetEntityName(grid.Value.Owner, $"HELLGRID-{name}");

        return (mapUid, grid.Value.Owner);
    }

}
