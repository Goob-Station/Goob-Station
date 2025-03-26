using Content.Server._Goobstation.Spawn.Components;
using Content.Server.Station.Systems;

namespace Content.Server._Goobstation.Spawn.Systems;

public sealed partial class UniqueEntitySystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UniqueEntityCheckerComponent, ComponentInit>(OnComponentInit);
    }

    public void OnComponentInit(Entity<UniqueEntityCheckerComponent> checker, ref ComponentInit args)
    {
        var comp = checker.Comp;

        if (string.IsNullOrEmpty(comp.MarkerName))
            return;

        var query = EntityQueryEnumerator<UniqueEntityMarkerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var marker, out var xform))
        {
            if (string.IsNullOrEmpty(marker.MarkerName)
                || marker.MarkerName != comp.MarkerName
                || uid == checker.Owner)
                continue;

            // Check if marker on station
            if (marker.StationOnly && _station.GetOwningStation(uid, xform) is null)
                continue;

            // Delete it if found unique entity
            QueueDel(checker);
            return;
        }
    }
}
