using Content.Server._Goobstation.Blob.Components;

namespace Content.Server._Goobstation.Objectives;

[RegisterComponent]
public sealed partial class BlobCaptureConditionComponent : Component
{
    [DataField]
    public int Target { get; set; } = StationBlobConfigComponent.DefaultStageEnd;
}
