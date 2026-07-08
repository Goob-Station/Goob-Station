// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Documents
{
    [RegisterComponent]
    public sealed partial class RandomDocumentComponent : Component
    {
        [DataField(required: true)]
        public ProtoId<DocumentTypePrototype> DocumentType = default!;

        [DataField]
        public List<ProtoId<NtrTaskPrototype>> Tasks = new();
    }
}
