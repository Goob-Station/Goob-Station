// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Documents
{
    [Prototype("spamType")]
    public sealed class SpamTypePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("legitDocuments")]
        public List<ProtoId<DocumentTypePrototype>> LegitDocuments { get; } = new();

        [DataField("mimicDocuments")]
        public List<ProtoId<DocumentTypePrototype>> MimicDocuments { get; } = new();

        [DataField("mimicChance")]
        public float MimicChance { get; } = 0.2f;
    }
}
