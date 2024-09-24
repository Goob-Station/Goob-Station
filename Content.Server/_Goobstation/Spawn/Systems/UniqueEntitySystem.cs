using Content.Server.Goobstation.Spawn.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;

namespace Content.Server.Goobstation.Spawn.Systems;

public sealed partial class UniqueEntitySystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UniqueEntityCheckerComponent, MapInitEvent>(OnMapInit);
    }

    public void OnMapInit(Entity<UniqueEntityCheckerComponent> checker, ref MapInitEvent args)
    {
        var comp = checker.Comp;

        if (string.IsNullOrEmpty(comp.MarkerName))
            return;

        var query = EntityQueryEnumerator<UniqueEntityMarkerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var marker, out var xform))
        {
            if (string.IsNullOrEmpty(marker.MarkerName) || marker.MarkerName != comp.MarkerName)
                continue;

            // Check if marker on station
            if (comp.StationOnly
                && (_station.GetOwningStation(uid, xform) is not { } station
                || !HasComp<StationDataComponent>(station)))
                continue;

            // Delete it if we found unique entity
            QueueDel(checker);
            return;
        }
    }
}
