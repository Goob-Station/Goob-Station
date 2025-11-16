using System.Linq;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Events;
using Content.Goobstation.Shared.Blob.Systems.Observer;

namespace Content.Goobstation.Server.Blob.Systems;

public sealed class BlobObserverSystem : SharedBlobObserverSystem
{

    private void OnCreateBlobObserver(EntityUid blobCoreUid, BlobCoreComponent core, CreateBlobObserverEvent args)
    {
        var observer = Spawn(core.ProjectionProtoId, Transform(blobCoreUid).Coordinates);

        core.Projection = observer;

        if (!TryComp<BlobProjectionComponent>(observer, out var blobObserverComponent))
        {
            args.Cancel();
            return;
        }

        blobObserverComponent.Core = blobCoreUid;
        Dirty(observer,blobObserverComponent);

        var blobRule = EntityQuery<BlobRuleComponent>().FirstOrDefault();
        blobRule?.Blobs.Add((mindId,mind));
    }
}
