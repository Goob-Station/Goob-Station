// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Style;
[Prototype("styleRank")]
public sealed class StyleRankPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("text")]
    public string DisplayText { get; } = string.Empty;

    [DataField]
    public Color Color { get; } = Color.White;

    [DataField]
    public float PointsRequired { get; } = 0f;

    [DataField]
    public float Multiplier { get; } = 1.0f;
}
