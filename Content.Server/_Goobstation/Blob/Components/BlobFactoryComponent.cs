using Content.Shared._Goobstation.Blob.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Blob.Components;

[RegisterComponent]
public sealed partial class BlobFactoryComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public float SpawnedCount = 0;

    [DataField("spawnLimit"), ViewVariables(VVAccess.ReadWrite)]
    public float SpawnLimit = 3;

    [DataField("blobSporeId"), ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId<BlobMobComponent> Pod = "MobBlobPod";

    [DataField("blobbernautId"), ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId<BlobbernautComponent> BlobbernautId = "MobBlobBlobbernaut";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Blobbernaut = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> BlobPods = new ();

    [DataField]
    public int Accumulator = 0;

    [DataField]
    public int AccumulateToSpawn = 3;
}

public sealed class ProduceBlobbernautEvent : EntityEventArgs
{
}
