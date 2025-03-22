using Content.Server._Goobstation.Teleportation.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Warps;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Server.GameObjects;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;

namespace Content.Server._Goobstation.Teleportation.EntitySystems;

public sealed class GoobLifelineSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarpParentOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(EntityUid uid, WarpParentOnTriggerComponent component, TriggerEvent args)
    {
        WarpParent(uid, component);
        args.Handled = true;
    }
    private void WarpParent(EntityUid uid, WarpParentOnTriggerComponent component)
    {
        var location = FindWarpPoint(component.WarpLocation);
        if (location == null)
            return;

        if (!TryComp<TransformComponent>(uid, out var transform))
            return;

        var parentUid = transform.ParentUid;
        if (parentUid == EntityUid.Invalid || !HasComp<MobStateComponent>(parentUid))
            return;

        RaiseLocalEvent(parentUid, new DropHandItemsEvent());
        _pullingSystem.StopAllPulls(parentUid);

        var coords = _transform.GetMapCoordinates(location.Value);
        if (transform.MapID != coords.MapId)
        {
            _transform.SetMapCoordinates(parentUid, coords);
        }

        QueueDel(uid);
    }
    private EntityUid? FindWarpPoint(string location)
    {
        var query = EntityQueryEnumerator<WarpPointComponent, TransformComponent>();

        while (query.MoveNext(out var entity, out var warp, out var transform))
        {
            if (warp.Location == location)
                return entity;
        }

        return null;
    }
}
