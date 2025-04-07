using Content.Goobstation.Server.Blob.Components;

namespace Content.Goobstation.Server.Blob.Objectives;

[RegisterComponent]
public sealed partial class BlobCaptureConditionComponent : Component
{
    [DataField]
    public int Target { get; set; } = StationBlobConfigComponent.DefaultStageEnd;
}
