using Content.Shared.Blob.Components;
using Content.Shared.Blob.NPC.BlobPod;

namespace Content.Client._Goobstation.Blob;

public sealed class BlobPodSystem : SharedBlobPodSystem
{
    public override bool NpcStartZombify(EntityUid uid, EntityUid argsTarget, BlobPodComponent component)
    {
        // do nothing
        return false;
    }
}
