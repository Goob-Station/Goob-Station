using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent]
public sealed partial class BlobFactoryComponent : Component
{
    [DataField]
    public int MaxPods = 3;

    [DataField]
    public int AccumulateToSpawn = 4;

    [DataField]
    public EntProtoId<BlobMobComponent> Pod = "MobBlobPod";

    [DataField]
    public EntProtoId<BlobbernautComponent> BlobbernautId = "MobBlobBlobbernaut";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Blobbernaut = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> BlobPods = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public int SpawnedCount = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public int Accumulator;
}

public sealed class ProduceBlobbernautEvent : EntityEventArgs;
