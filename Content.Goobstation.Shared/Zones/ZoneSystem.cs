using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Zones;

/// <summary>
/// Zone system, for all your zoning needs
/// This is probably not very performance friendly? maybe cache results for tick
/// </summary>
public sealed partial class ZoneSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    [PublicAPI]
    public bool IsWithin(Entity<TransformComponent?> ent, ZonePrototype zone)
    {
        // isnt it so cool how using collection initialization results in a sandbox violation here some fucking how
        List<ZonePrototype> list = new();
        list.Add(zone);
        return IsWithinAny(ent, list);
    }

    [PublicAPI]
    public bool IsWithinAny(Entity<TransformComponent?> ent, List<ZonePrototype> zones)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (zones.Count == 0)
            return false;

        var entXform = Transform(ent);
        var entMapCoords = _xform.GetMapCoordinates(ent, entXform);

        var query = EntityQueryEnumerator<ZoneMarkerComponent, TransformComponent>();
        while (query.MoveNext(out var markerUid, out var markerComp, out var markerXform))
        {
            if (!zones.Contains(_proto.Index(markerComp.ZoneId)))
                continue;

            if (entXform.MapID != markerXform.MapID)
                continue;

            var markerMapCoords = _xform.GetMapCoordinates(markerUid, markerXform);
            if (!markerMapCoords.InRange(entMapCoords, markerComp.Range))
                continue;

            return true;
        }

        return false;
    }

    [PublicAPI]
    public bool IsWithin(Entity<TransformComponent?> ent, ProtoId<ZonePrototype> zoneId)
    {
        return IsWithin(ent, _proto.Index(zoneId));
    }

    [PublicAPI]
    public bool IsWithinAny(Entity<TransformComponent?> ent, List<ProtoId<ZonePrototype>> zoneIds)
    {
        List<ZonePrototype> resolvedZones = [];
        foreach (var zoneId in zoneIds)
        {
            resolvedZones.Add(_proto.Index(zoneId));
        }
        return IsWithinAny(ent, resolvedZones);
    }
}
