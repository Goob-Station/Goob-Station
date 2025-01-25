using System.Threading;
using System.Threading.Tasks;
using Content.Shared.ActionBlocker;
using Content.Shared._Goobstation.Blob.Components;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobObserverMover : Job<object>
{
    public BlobObserverMover(EntityManager entityManager, ActionBlockerSystem blockerSystem, SharedTransformSystem transform, BlobObserverSystem observerSystem, double maxTime, CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _observerSystem = observerSystem;
        _transform = transform;
        //_blocker = blockerSystem;
        _entityManager = entityManager;
    }

    public BlobObserverMover(EntityManager entityManager, ActionBlockerSystem blockerSystem, SharedTransformSystem transform, BlobObserverSystem observerSystem, double maxTime, IStopwatch stopwatch, CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
    {
        _observerSystem = observerSystem;
        _transform = transform;
        //_blocker = blockerSystem;
        _entityManager = entityManager;
    }
    public EntityCoordinates NewPosition;
    public Entity<BlobObserverComponent> Observer;

    private BlobObserverSystem _observerSystem;
    private SharedTransformSystem _transform;
    //private ActionBlockerSystem _blocker;
    private EntityManager _entityManager;


    protected override async Task<object?> Process()
    {
        try
        {
            if (Observer.Comp.Core == null)
            {
                return default;
            }

            var newPos = _transform.ToMapCoordinates(NewPosition);

            var (nearestEntityUid, nearestDistance) = _observerSystem.CalculateNearestBlobTileDistance(newPos);

            if (nearestEntityUid == null)
                return default;

            if (nearestDistance > 5f)
            {
                if (_entityManager.Deleted(Observer.Comp.Core.Value) ||
                    !_entityManager.TryGetComponent<TransformComponent>(Observer.Comp.Core.Value, out var xform))
                {
                    _entityManager.QueueDeleteEntity(Observer);
                    return default;
                }

                _transform.SetCoordinates(Observer, xform.Coordinates);
                return default;
            }

            if (nearestDistance > 3f)
            {
                /*Observer.Comp.CanMove = false;
                _blocker.UpdateCanMove(Observer);*/

                var nearestEntityPos = _transform.GetMapCoordinates(nearestEntityUid.Value);

                var direction = (nearestEntityPos.Position - newPos.Position);
                var newPosition = newPos.Offset(direction * 0.1f);

                _transform.SetMapCoordinates(Observer, newPosition);
                return default;
            }

            /*if (!Observer.Comp.CanMove)
            {
                Observer.Comp.CanMove = true;
                _blocker.UpdateCanMove(Observer);
            }*/

            return default;
        }
        finally
        {
            Observer.Comp.IsProcessingMoveEvent = false;
        }
    }
}
