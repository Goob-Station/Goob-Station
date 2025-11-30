using Content.Goobstation.Shared.Blob.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent]
public sealed partial class BlobUpgradeableTileComponent : Component
{
    [DataField]
    public ProtoId<BlobTilePrototype> TransformTo;

    [DataField]
    public LocId Locale;
}
