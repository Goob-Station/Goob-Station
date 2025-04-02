namespace Content.Goobstation.Server.NTR.Scan
{
    [RegisterComponent]
    public sealed partial class ScannableForPointsComponent : Component
    {
        [DataField("points")]
        public int Points = 5;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool AlreadyScanned = false;
    }
}
