namespace Content.Goobstation.Shared.NTR.Documents
{
    [RegisterComponent]
    public sealed partial class SpamDocumentComponent : Component
    {
        [DataField]
        public SpamType stype = SpamType.Obvious;

        public enum SpamType
        {
            Obvious,
            Mimic
        }
    }
}
