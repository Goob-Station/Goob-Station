using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.StoryGen;

namespace Content.Server._Goobstation.NTR
{
    [RegisterComponent]
    public sealed partial class RandomDocumentComponent : Component
    {
        [DataField]
        public DocumentType dtype = DocumentType.Service;

        public enum DocumentType
        {
            Service,
            Security,
            Cargo,
            Medical,
            Engineering
        }
    }
}
