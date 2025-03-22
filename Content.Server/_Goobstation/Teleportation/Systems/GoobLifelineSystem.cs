using Content.Server._Goobstation.Teleportation.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Warps;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Server.GameObjects;
using Content.Shared.Rejuvenate;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Server._Goobstation.Teleportation.EntitySystems;

public sealed class GoobLifelineSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarpParentOnTriggerComponent, TriggerEvent>(HandleWarpParentTrigger);

        var query = _entManager.AllEntityQueryEnumerator<WarpParentOnTriggerComponent>();
        if (query.MoveNext(out var entity, out _))
        {
            ForcedLocation(entity);
        }
    }

    private void HandleWarpParentTrigger(EntityUid uid, WarpParentOnTriggerComponent component, TriggerEvent args)
    {
        var location = FindWarpPoint(component.WarpLocation);
        if (location == null)
        {
            return;
        }

        if (!TryComp<TransformComponent>(uid, out var transform))
        {
            return;
        }

        var parentUid = transform.ParentUid;
        if (parentUid == EntityUid.Invalid)
        {
            return;
        }

        // Drop items from hands
        if (TryComp<HandsComponent>(parentUid, out var hands))
        {
            foreach (var hand in hands.Hands.Values)
            {
                var heldEntity = hand.HeldEntity;
                if (heldEntity != null)
                {
                    var dropEvent = new VirtualItemDropAttemptEvent(parentUid, parentUid, heldEntity.Value, false);
                    RaiseLocalEvent(parentUid, dropEvent);

                    if (!dropEvent.Cancelled)
                    {
                        _entManager.GetComponent<TransformComponent>(heldEntity.Value).Coordinates = _entManager.GetComponent<TransformComponent>(parentUid).Coordinates;
                        Get<SharedHandsSystem>().TryDrop(parentUid, hand);
                    }
                }
            }
        }

        _pullingSystem.StopAllPulls(parentUid);

        var coords = _transform.GetMapCoordinates(location.Value);

        if (transform.MapID != coords.MapId)
        {
            _transform.SetMapCoordinates(parentUid, coords);
        }
        else
        {
            _transform.SetMapCoordinates(parentUid, coords);
        }

        if (!TryComp<TransformComponent>(parentUid, out var parentTransform) || parentTransform.GridUid == EntityUid.Invalid)
        {
            _transform.AttachToGridOrMap(parentUid);
        }

        RaiseLocalEvent(parentUid, new RejuvenateEvent());

        args.Handled = true;
    }

    public void ForcedLocation(EntityUid entity)
    {
        if (!TryComp<WarpParentOnTriggerComponent>(entity, out var component))
        {
            return;
        }

        var location = FindWarpPoint(component.WarpLocation);
        if (location == null)
        {
            return;
        }

        HandleWarpParentTrigger(entity, component, new TriggerEvent(entity));
    }

    private EntityUid? FindWarpPoint(string location)
    {
        var query = _entManager.AllEntityQueryEnumerator<WarpPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var warp, out _))
        {
            if (warp.Location == location)
            {
                return uid;
            }
        }
        return null;
    }
}
