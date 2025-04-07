// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Blob;
using Content.Server.Abilities.Felinid;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Nutrition.Components;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Blob.StationEvents;

public sealed class BlobSpawnRule : StationEventSystem<BlobSpawnRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlobCarrierComponent, GhostRoleSpawnerUsedEvent>(OnSpawned);
    }

    protected override void Started(EntityUid uid,
        BlobSpawnRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station))
        {
            return;
        }

        var locations = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        var validLocations = new List<EntityCoordinates>();
        while (locations.MoveNext(out _, out _, out var transform))
        {
            if (!HasComp<BecomesStationComponent>(transform.GridUid))
                continue;

            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == station)
            {
                validLocations.Add(transform.Coordinates);
            }
        }

        if (validLocations.Count == 0)
        {
            Sawmill.Warning("There was no valid spawn points for blob!");
            return;
        }

        var playerPool = _playerSystem.Sessions.ToList();
        var numBlobs = MathHelper.Clamp(playerPool.Count / component.PlayersPerCarrierBlob, 1, component.MaxCarrierBlob);

        for (var i = 0; i < numBlobs; i++)
        {
            var coords = _random.Pick(validLocations);
            Sawmill.Info($"Creating carrier blob at {coords}");
            Spawn(_random.Pick(component.CarrierBlobProtos), coords);
        }

        // start blob rule incase it isn't, for the sweet greentext
        GameTicker.StartGameRule("BlobRule");
    }

    // Because GameRule spawns just a GhostRoleSpawner, we can't just remove components
    // right away, and need to track the event when entity is spawned.
    private void OnSpawned(EntityUid uid, BlobCarrierComponent component, GhostRoleSpawnerUsedEvent args)
    {
        var carrier = args.Spawned;
        if (!TryComp<BlobCarrierComponent>(carrier, out _))
            return;

        // Blob doesn't spawn when blob carrier was eaten.
        RemComp<FoodComponent>(carrier);
        RemComp<FelinidFoodComponent>(carrier);


    }
}