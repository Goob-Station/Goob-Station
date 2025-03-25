using System.Numerics;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Interaction;
using Robust.Shared.Map;

namespace Content.Shared._Goobstation.Blob;

public abstract class SharedBlobObserverSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<BlobObserverComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<BlobObserverComponent, GetUsedEntityEvent>(OnGetUsedEntityEvent);
    }

    private void OnGetUsedEntityEvent(Entity<BlobObserverComponent> ent, ref GetUsedEntityEvent args)
    {
        if(ent.Comp.VirtualItem.Valid)
            args.Used = ent.Comp.VirtualItem;
    }

    /*private void OnUpdateCanMove(EntityUid uid, BlobObserverComponent component, UpdateCanMoveEvent args)
    {
        if (component.CanMove)
            return;

        args.Cancel();
    }*/

    public (EntityUid? nearestEntityUid, float nearestDistance) CalculateNearestBlobTileDistance(MapCoordinates position)
    {
        var nearestDistance = float.MaxValue;
        EntityUid? nearestEntityUid = null;

        foreach (var lookupUid in _lookup.GetEntitiesInRange<BlobTileComponent>(position, 5f))
        {
            var tileCords = _transform.GetMapCoordinates(lookupUid);
            var distance = Vector2.Distance(position.Position, tileCords.Position);

            if (!(distance < nearestDistance))
                continue;

            nearestDistance = distance;
            nearestEntityUid = lookupUid;
        }

        return (nearestEntityUid, nearestDistance);
    }
}
