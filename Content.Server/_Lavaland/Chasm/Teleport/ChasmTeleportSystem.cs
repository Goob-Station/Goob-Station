using Content.Shared._Lavaland.Chasm;
using Content.Shared._Lavaland.Chasm.Teleport;
using Content.Shared.Chasm;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Map;

namespace Content.Server._Lavaland.Chasm.Teleport;

/// <summary>
/// Teleport player onto grid upon falling in chasm. Loads grid if grid isn't loaded in yet.
/// </summary>
public sealed class ChasmTeleportSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChasmFallingComponent, BeforeChasmFallingEvent>(OnBeforeFalling);
    }

    private void OnBeforeFalling(EntityUid uid, ChasmFallingComponent falling, ref BeforeChasmFallingEvent args)
    {
        if (args.Cancelled)
            return;

        if (falling.SourceChasm == null || !TryComp<ChasmTeleportComponent>(falling.SourceChasm.Value, out var comp))
            return;

        if (!TryGetOrLoadMap(comp, out var beaconCoords))
            return;

        args.Cancelled = true;
        _transform.SetCoordinates(args.Entity, beaconCoords);
    }

    private bool TryGetOrLoadMap(ChasmTeleportComponent comp, out EntityCoordinates beaconCoords)
    {
        beaconCoords = default;

        if (!TerminatingOrDeleted(comp.LoadedMap))
            return TryGetBeaconCoords(comp.LoadedMap.Value, out beaconCoords);

        if (!_mapLoader.TryLoadMap(comp.MapPath, out var map, out var roots, options: new DeserializationOptions { InitializeMaps = true }))
        {
            Log.Error($"ChasmTeleportSystem didn't manage to load {comp.MapPath}");
            return false;
        }

        comp.LoadedMap = map;
        return TryGetBeaconCoords(map!.Value, out beaconCoords);
    }

    private bool TryGetBeaconCoords(EntityUid mapUid, [NotNullWhen(true)] out EntityCoordinates? coords)
    {
        var query = EntityQueryEnumerator<ChasmTeleportBeaconComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (Transform(uid).MapUid != mapUid)
                continue;

            coords = xform.Coordinates;
            return true;
        }

        coords = null;
        return false;
    }
}
