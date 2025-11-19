// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Documents
{
    [Prototype("documentType")]
    public sealed class DocumentTypePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public string StartingText { get; } = string.Empty;

        [DataField]
        public string Template { get; } = string.Empty;

        [DataField]
        public string[] TextKeys { get; } = Array.Empty<string>();

        [DataField]
        public int[] TextCounts { get; } = Array.Empty<int>();
    }
}
