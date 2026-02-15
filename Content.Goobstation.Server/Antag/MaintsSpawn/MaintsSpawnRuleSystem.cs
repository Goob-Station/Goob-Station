using Content.Server.Antag;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Antag.MaintsSpawn;

public sealed class MaintsSpawnRule : StationEventSystem<MaintsSpawnRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MaintsSpawnRuleComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    protected override void Added(EntityUid uid, MaintsSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station))
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        var locations = EntityQueryEnumerator<MaintsSpawnLocationComponent, TransformComponent>();
        var validLocations = new List<MapCoordinates>();
        while (locations.MoveNext(out _, out _, out var transform))
        {
            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station != station)
                continue;

            validLocations.Add(_transform.GetMapCoordinates(transform));
        }

        if (validLocations.Count == 0)
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        component.Coords = validLocations;
    }

    private void OnSelectLocation(Entity<MaintsSpawnRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (ent.Comp.Coords is {} coords)
            args.Coordinates.AddRange(coords);
    }
}
