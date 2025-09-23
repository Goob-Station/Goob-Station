// SPDX-License-Identifier: AGPL-3.0-or-later
// Always-active portal that teleports the user into a custom "hell" map.

using Content.Goobstation.Shared.Teleportation.Components;
using Content.Goobstation.Shared.Maps;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using System.Linq;

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
            // If the map exists, ensure the ExitPortal is valid before linking
            if (comp.ExitPortal != default && EntityManager.EntityExists(comp.ExitPortal))
            {
                _link.TryLink(uid, comp.ExitPortal);
            }
            return;
        }
        // Load the hell map if it doesn't exist
        if (!_mapLoader.TryLoadMap(new ResPath("/Maps/_Goobstation/Nonstations/Hell.yml"),
                out var map, out var roots,
                options: new Robust.Shared.EntitySerialization.DeserializationOptions { InitializeMaps = true }))
        {
            Log.Error("Failed to load hell map at /Maps/_Goobstation/Nonstations/Hell.yml");
            QueueDel(map);
            return;
        }

        // Add HellMapComponent to mark the map as loaded
        EnsureComp<HellMapComponent>(map.Value);

        comp.HellMap = map;

        foreach (var root in roots)
        {
            if (!HasComp<MapGridComponent>(root))
                continue;

            var pos = new EntityCoordinates(root, 0, 0);

            // Ensure the placed entity itself is a portal
            EnsureComp<PortalComponent>(uid, out var portalComp);
            EnsureComp<LinkedEntityComponent>(uid);

            portalComp.CanTeleportToOtherMaps = true;
            if (string.IsNullOrEmpty(comp.ExitPortalPrototype))
            {
                Log.Error("HellPortalComponent.ExitPortalPrototype is null or empty");
                return;
            }
            // Spawn a receiver portal inside hell
            comp.ExitPortal = Spawn(comp.ExitPortalPrototype, pos);
            if (comp.ExitPortal == default)
            {
                Log.Error("Failed to spawn hell exit portal");
                return;
            }
            EnsureComp<PortalComponent>(comp.ExitPortal, out var hellPortalComp);
            EnsureComp<LinkedEntityComponent>(comp.ExitPortal);

            // Permanently link both ways
            _link.TryLink(uid, comp.ExitPortal);

            break;
        }
    }
}
