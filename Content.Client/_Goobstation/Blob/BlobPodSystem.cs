using Content.Shared._Goobstation.Blob.Components;
using Content.Shared._Goobstation.Blob.NPC.BlobPod;

namespace Content.Client._Goobstation.Blob;

public sealed class BlobPodSystem : SharedBlobPodSystem
{
    public override bool NpcStartZombify(EntityUid uid, EntityUid argsTarget, BlobPodComponent component)
    {
        // do nothing
        return false;
    }
}
