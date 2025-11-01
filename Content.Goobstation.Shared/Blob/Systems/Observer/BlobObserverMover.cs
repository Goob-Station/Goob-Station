using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Blob.Components;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Blob.Systems.Observer;

/// <summary>
/// Handles restricting blob's movement to the area where blob tiles exist.
/// </summary>
public sealed class BlobObserverMover(
    EntityManager entityManager,
    SharedTransformSystem transform,
    SharedBlobObserverSystem observerSystem,
    double maxTime,
    CancellationToken cancellation = default)
    : Job<object>(maxTime, cancellation)
{
    public EntityCoordinates NewPosition;
    public Entity<BlobProjectionComponent> Observer;

    protected override Task<object?> Process()
    {
        try
        {
            if (Observer.Comp.Core == null)
                return Task.FromResult<object?>(null);

            var newPos = transform.ToMapCoordinates(NewPosition);

            var (nearestEntityUid, nearestDistance) = observerSystem.CalculateNearestBlobTileDistance(newPos);

            if (nearestEntityUid == null)
                return Task.FromResult<object?>(null);

            if (nearestDistance > 5f)
            {
                if (entityManager.Deleted(Observer.Comp.Core.Value) ||
                    !entityManager.TryGetComponent<TransformComponent>(Observer.Comp.Core.Value, out var xform))
                {
                    entityManager.QueueDeleteEntity(Observer);
                    return Task.FromResult<object?>(null);
                }

                transform.SetCoordinates(Observer, xform.Coordinates);
                return Task.FromResult<object?>(null);
            }

            if (nearestDistance > 3f)
            {
                var nearestEntityPos = transform.GetMapCoordinates(nearestEntityUid.Value);

                var direction = (nearestEntityPos.Position - newPos.Position);
                var newPosition = newPos.Offset(direction * 0.1f);

                transform.SetMapCoordinates(Observer, newPosition);
                return Task.FromResult<object?>(null);
            }

            return Task.FromResult<object?>(null);
        }
        finally
        {
            Observer.Comp.IsProcessingMoveEvent = false;
        }
    }
}
