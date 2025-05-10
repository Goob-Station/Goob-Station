// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
