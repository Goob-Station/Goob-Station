using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.StoryGen;

namespace Content.Goobstation.Server.NTR.Documents
{
    [RegisterComponent]
    public sealed partial class RandomDocumentComponent : Component
    {
        [DataField]
        public DocumentType dtype = DocumentType.Service; //default to service

        public enum DocumentType
        {
            Service,
            Security,
            Cargo,
            Medical,
            Engineering,
            Science,
        }
    }
}
