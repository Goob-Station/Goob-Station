using System.Linq;
using Content.Shared.EntityTable;
using Content.Shared.Interaction.Events;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.DropPod;

public sealed partial class DropPodSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;

    public static readonly EntProtoId DefaultPod = "DropPod";

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<DropPodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LandTime = _timing.CurTime + ent.Comp.LandDelay;
    }

    [SubscribeLocalEvent]
    private void OnLauncherUse(Entity<DropPodLauncherComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        if (_net.IsClient)
            return;

        var passengers = new List<EntityUid>();
        foreach (var id in ent.Comp.Containers)
        {
            if (_container.TryGetContainer(ent, id, out var container))
                passengers.AddRange(container.ContainedEntities);
        }

        Launch(_xform.GetMoverCoordinates(args.User), existing: passengers, pod: ent.Comp.Pod);
        QueueDel(ent);
    }

    public EntityUid? Launch(EntityCoordinates coordinates,
        IEnumerable<EntProtoId>? contents = null,
        IEnumerable<EntityUid>? existing = null,
        EntProtoId? pod = null)
    {
        if (_net.IsClient)
            return null;

        var podUid = SpawnAtPosition(pod ?? DefaultPod, coordinates);
        var comp = EnsureComp<DropPodComponent>(podUid);
        if (contents != null)
            comp.Contents.AddRange(contents);

        if (existing == null)
            return podUid;

        var container = _container.EnsureContainer<Container>(podUid, DropPodComponent.ContainerId);
        foreach (var entity in existing.ToList())
        {
            _container.TryRemoveFromContainer(entity, true);
            if (Transform(entity).Anchored)
                _xform.Unanchor(entity);

            _container.Insert(entity, container);
        }

        return podUid;
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var now = _timing.CurTime;
        var landed = new List<(EntityUid Uid, DropPodComponent Pod, EntityCoordinates Coords)>();
        var query = EntityQueryEnumerator<DropPodComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var pod, out var xform))
        {
            if (now >= pod.LandTime)
                landed.Add((uid, pod, xform.Coordinates));
        }

        foreach (var (uid, pod, coords) in landed)
            Land(uid, pod, coords);
    }

    private void Land(EntityUid uid, DropPodComponent pod, EntityCoordinates coords)
    {
        foreach (var proto in pod.Contents)
            SpawnAtPosition(proto, coords);

        if (pod.Table != null)
        {
            foreach (var proto in _entityTable.GetSpawns(pod.Table))
                SpawnAtPosition(proto, coords);
        }

        if (_container.TryGetContainer(uid, DropPodComponent.ContainerId, out var container))
            _container.EmptyContainer(container, true, coords);

        QueueDel(uid);
    }
}
