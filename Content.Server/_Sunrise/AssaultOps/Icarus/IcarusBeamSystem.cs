using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Ghost;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Sunrise.AssaultOps.Icarus;

public sealed class IcarusBeamSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQuery<IcarusBeamComponent, TransformComponent>(true);
        foreach (var (comp, xform) in query)
        {
            DestroyEntities(comp, xform);
            BurnEntities(comp.Owner, comp, xform);

            if (comp.DestroyTiles)
                DestroyTiles(comp, xform);

            if (_timing.CurTime > comp.LifetimeEnd)
                QueueDel(comp.Owner);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IcarusBeamComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, IcarusBeamComponent component, ComponentInit args)
    {
        component.LifetimeEnd = _timing.CurTime + component.Lifetime;
        if (!TryComp(uid, out PhysicsComponent? phys))
            return;
        _physics.SetLinearDamping(uid, phys, 0f);
        _physics.SetFriction(uid, phys, 0f);
        _physics.SetAngularDamping(uid, phys, 0f);
    }

    public void LaunchInDirection(EntityUid uid, Vector2 dir, IcarusBeamComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;


        if (TryComp(uid, out PhysicsComponent? phys))
        {
            var impulseVector = dir.Normalized() * comp.Speed * phys.Mass;

            _physics.ApplyLinearImpulse(uid, impulseVector, body: phys);
            _transform.SetWorldRotation(uid, impulseVector.ToWorldAngle());
        }
    }

    /// <summary>
    /// Destroy any grid tiles in beam radius.
    /// </summary>
    private void DestroyTiles(IcarusBeamComponent component, TransformComponent trans)
    {
        var radius = component.DestroyRadius;
        var worldPos = trans.WorldPosition;

        var circle = new Circle(worldPos, radius);
        var r = new Vector2(radius, radius);
        var box = new Box2(worldPos - r, worldPos + r);

        foreach (var grid in _map.FindGridsIntersecting(trans.MapID, box))
        {
            // Bundle these together so we can use the faster helper to set tiles.
            var toDestroy = new List<(Vector2i, Tile)>();

            foreach (var tile in grid.GetTilesIntersecting(circle))
            {
                if (tile.Tile.IsEmpty)
                    continue;

                toDestroy.Add((tile.GridIndices, Tile.Empty));
            }

            grid.SetTiles(toDestroy);
        }
    }

    /// <summary>
    /// Handle deleting entities in beam radius.
    /// </summary>
    private void DestroyEntities(IcarusBeamComponent component, TransformComponent trans)
    {
        var radius = component.DestroyRadius - 0.5f;
        var entitys = _lookup.GetEntitiesInRange(trans.MapID, trans.WorldPosition, radius);
        foreach (var entity in entitys)
        {
            if (!CanDestroy(component, entity))
                continue;

            QueueDel(entity);
        }
    }

    /// <summary>
    /// Handle igniting flammable entities in beam radius.
    /// </summary>
    private void BurnEntities(EntityUid beam, IcarusBeamComponent component, TransformComponent trans)
    {
        var radius = component.FlameRadius * 2;
        foreach (var entity in _lookup.GetEntitiesInRange(trans.MapID, trans.WorldPosition, radius))
        {
            if (!CanDestroy(component, entity))
                continue;

            if (!TryComp<FlammableComponent>(entity, out var flammable))
                continue;

            flammable.FireStacks += 1;
            if (!flammable.OnFire)
                _flammable.Ignite(entity, beam);
        }
    }

    private bool CanDestroy(IcarusBeamComponent component, EntityUid entity)
    {
        return entity != component.Owner &&
               !EntityManager.HasComponent<MapGridComponent>(entity) &&
               !EntityManager.HasComponent<GhostComponent>(entity);
    }
}
