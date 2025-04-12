// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Construction
{
    [Serializable, NetSerializable]
    public sealed class ConstructionGuide
    {
        public readonly ConstructionGuideEntry[] Entries;

        public ConstructionGuide(ConstructionGuideEntry[] entries)
        {
            Entries = entries;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ConstructionGuideEntry
    {
        public int? EntryNumber { get; set; } = null;
        public int Padding { get; set; } = 0;
        public string Localization { get; set; } = string.Empty;
        public (string, object)[]? Arguments { get; set; } = null;
        public SpriteSpecifier? Icon { get; set; } = null;
    }
}