using Content.Server._Goobstation.Blob.Components;

namespace Content.Server.Objectives;

[RegisterComponent]
public sealed partial class BlobCaptureConditionComponent : Component
{
    [DataField]
    public int Target { get; set; } = StationBlobConfigComponent.DefaultStageEnd;
}
