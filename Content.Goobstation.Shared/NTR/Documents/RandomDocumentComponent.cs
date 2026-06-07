// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
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
