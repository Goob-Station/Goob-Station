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

namespace Content.Goobstation.Shared.Teleportation.Systems;

public sealed class HellPortalSystem : EntitySystem
{
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    EntityUid hellExit;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("hell_portal");

        SubscribeLocalEvent<HellPortalComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, HellPortalComponent comp, ComponentStartup args)
    {
        // Check if hell already exists
        var existingHellMaps = EntityQuery<HellMapComponent>();
        if (existingHellMaps.Any())
        {
            // Hell map already exists, just link the portal
            _link.TryLink(uid, hellExit);
            return;
        }

        // Load the hell map if it doesn't exist
        if (!_mapLoader.TryLoadMap(new ResPath("/Maps/_Goobstation/Nonstations/Hell.yml"),
                out var map, out var roots,
                options: new Robust.Shared.EntitySerialization.DeserializationOptions { InitializeMaps = true }))
        {
            _sawmill.Error("Failed to load hell map at /Maps/_Goobstation/Nonstations/Hell.yml");
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

            // Spawn a receiver portal inside hell
            hellExit = Spawn(comp.ExitPortalPrototype, pos);
            EnsureComp<PortalComponent>(hellExit, out var hellPortalComp);
            EnsureComp<LinkedEntityComponent>(hellExit);

            // Permanently link both ways
            _link.TryLink(uid, hellExit);

            break;
        }
    }
}
