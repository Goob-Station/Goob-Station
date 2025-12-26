using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Destructible;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;
using Robust.Shared.Physics.Components;
using Content.Goobstation.Server.Insurance.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Insurance.Systems;

public sealed partial class InsuranceSystem : EntitySystem
{
    [Dependency] private readonly SpawnOnDespawnSystem _spawnOnDespawn = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InsuranceComponent, DestructionEventArgs>(OnDestruct);
    }

    public override void Update(float frameTime)
    {
        UpdateScheduledDrops(frameTime);
    }

    private void OnDestruct(Entity<InsuranceComponent> ent, ref DestructionEventArgs args)
    {
        if (!Exists(ent.Comp.PolicyOwner))
            return;

        if (!TryComp(ent.Comp.PolicyOwner, out TransformComponent? xform))
            return;

        ScheduleDrop(ent);
    }

    #region Drop scheduling

    private LinkedList<ScheduledDrop> _scheduledDrops = [];

    private void ScheduleDrop(Entity<InsuranceComponent> ent)
    {
        var drop = new ScheduledDrop()
        {
            Target = ent.Comp.PolicyOwner,
            Policy = ent.Comp.Policy,
            TimeLeft = ent.Comp.Policy.DropDelay,
        };

        if (Prototype(ent) is EntityPrototype proto)
            drop.Proto = proto;

        if (ent.Comp.Policy.DropDelay == null)
            Drop(drop);
        else
            _scheduledDrops.AddFirst(drop);
    }

    private void UpdateScheduledDrops(float frameTime)
    {
        var node = _scheduledDrops.First;
        while (node != null)
        {
            var next = node.Next;
            if (node.Value.TimeLeft != null)
            {
                node.Value.TimeLeft -= frameTime;
                if (node.Value.TimeLeft < 0)
                {
                    Drop(node.Value);
                    _scheduledDrops.Remove(node);
                }
            }
            node = next;
        }
    }

    private sealed class ScheduledDrop
    {
        public required EntityUid Target;

        public required InsurancePolicy Policy;

        public EntProtoId? Proto;

        public float? TimeLeft;
    }

    #endregion

    #region Dropping

    private void Drop(ScheduledDrop drop)
    {
        if (!Exists(drop.Target))
            return;

        if (!TryComp(drop.Target, out TransformComponent? xform))
            return;

        List<EntProtoId> protos = [];

        if (drop.Policy.IncludeTarget && drop.Proto != null)
            protos.Add(drop.Proto.Value);

        if (drop.Policy.ExtraCompensationItems != null)
            protos.AddRange(drop.Policy.ExtraCompensationItems);

        var dropPod = Spawn("SpawnSupplyEmpty", SelectDropPos(xform, drop.Policy.DropRadius));
        var spawnOnDespawn = EnsureComp<SpawnOnDespawnComponent>(dropPod);
        _spawnOnDespawn.SetPrototypes(new(dropPod, spawnOnDespawn), protos);
    }

    private MapCoordinates SelectDropPos(TransformComponent xform, int searchRadius)
    {
        if (xform.GridUid == null || !TryComp<MapGridComponent>(xform.GridUid.Value, out var grid))
            return _xform.ToMapCoordinates(xform.Coordinates);

        List<Vector2i> validTiles = [];

        var tileCoords = _map.CoordinatesToTile(xform.GridUid.Value, grid, xform.Coordinates);

        var physQuery = GetEntityQuery<PhysicsComponent>();

        for (int x = tileCoords.X - searchRadius; x <= tileCoords.X + searchRadius; x++)
        {
            for (int y = tileCoords.Y - searchRadius; y <= tileCoords.Y + searchRadius; y++)
            {
                Vector2i coords = new(x, y);

                if (!_map.TryGetTileRef(xform.GridUid.Value, grid, coords, out var tileRef) ||
                        _turf.IsSpace(tileRef))
                    continue;

                var valid = true;

                foreach (var ent in _map.GetAnchoredEntities(xform.GridUid.Value, grid, coords))
                {
                    if (!physQuery.TryGetComponent(ent, out var body))
                        continue;

                    if (body.Hard && (body.CollisionMask & (int) CollisionGroup.MobMask) != 0)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    validTiles.Add(coords);
            }
        }

        if (validTiles.Count > 0)
            return _map.GridTileToWorld(xform.GridUid.Value, grid, _random.Pick(validTiles));
        else
            return _xform.ToMapCoordinates(xform.Coordinates);
    }

    #endregion
}
