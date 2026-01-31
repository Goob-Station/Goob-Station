using Content.Server._White.StationEvents.Components;
using Content.Server.Antag;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Map;

namespace Content.Server._White.StationEvents.Events;

public sealed class VentSpawnRule : StationEventSystem<VentSpawnRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VentSpawnRuleComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    private void OnSelectLocation(Entity<VentSpawnRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        var comp = Comp<GameRuleComponent>(args.GameRule);

        if (!TryGetRandomStation(out var station))
        {
            ForceEndSelf(ent, comp);
            return;
        }

        var locations = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        var validLocations = new List<MapCoordinates>();
        while (locations.MoveNext(out _, out _, out var transform))
        {
            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station != station)
                continue;

            validLocations.Add(_transform.GetMapCoordinates(transform));
        }

        if (validLocations.Count == 0)
        {
            ForceEndSelf(ent, comp);
            return;
        }

        if (validLocations is { } coords)
        {
            args.Coordinates.AddRange(coords);
        }
    }
}
