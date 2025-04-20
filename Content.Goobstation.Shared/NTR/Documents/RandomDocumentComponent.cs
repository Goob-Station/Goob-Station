// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Documents
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
