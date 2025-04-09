namespace Content.Goobstation.Shared.NTR.Scan
{
    [RegisterComponent]
    public sealed partial class BriefcaseScannerComponent : Component
    {
        [DataField("scanDuration")]
        public float ScanDuration = 10f;
    }
}
