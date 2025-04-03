using Content.Goobstation.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR
{
    [RegisterComponent]
    public sealed partial class RandomDocumentComponent : Component
    {
        [DataField(required: true)]
        public List<ProtoId<NtrTaskPrototype>> Tasks = new();

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
