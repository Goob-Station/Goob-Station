using Content.Goobstation.Shared.Teleportation.Components;
using Content.Goobstation.Shared.Maps;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.HellGoose;

public sealed class HellPortalSystem : EntitySystem
{
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellPortalComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, HellPortalComponent comp, MapInitEvent args)
    {
        // Check if hell already exists
        var query = EntityQueryEnumerator<HellMapComponent>();
        if (query.MoveNext(out var hellMapUid, out var hellMapComp))
        {
            // If hell already has an exit portal, link to it
            if (hellMapComp.ExitPortal != null && !TerminatingOrDeleted(hellMapComp.ExitPortal.Value))
                _link.TryLink(uid, hellMapComp.ExitPortal.Value);
            return;
        }

        // Otherwise, load the hell map
        if (!_mapLoader.TryLoadMap(comp.HellMapPath,
                out var map, out var roots,
                options: new DeserializationOptions { InitializeMaps = true }))
        {
            Log.Error($"Failed to load hell map at {comp.HellMapPath}");
            QueueDel(map);
            return;
        }

        foreach (var root in roots)
        {
            if (!HasComp<HellMapComponent>(root))
                continue;

            var pos = new EntityCoordinates(root, 0, 0);

            var portalComp = EnsureComp<PortalComponent>(uid);
            EnsureComp<LinkedEntityComponent>(uid);

            portalComp.CanTeleportToOtherMaps = true;

            var exitPortal = Spawn(comp.ExitPortalPrototype, pos);

            EnsureComp<PortalComponent>(exitPortal, out var hellPortalComp);
            EnsureComp<LinkedEntityComponent>(exitPortal);

            // Save it in the hell map component (instance, not static)
            var newHellMapComp = EnsureComp<HellMapComponent>(root);
            newHellMapComp.ExitPortal = exitPortal;

            _link.TryLink(uid, exitPortal);
            break;
        }
    }
}
