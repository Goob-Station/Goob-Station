// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.WildLands;

/// <summary>
/// Kudzu that can only expand within <see cref="MaxRadius"/> tiles of its mother origin.
/// Server-only gameplay state — not networked.
/// </summary>
[RegisterComponent]
public sealed partial class BoundedKudzuComponent : Component
{
    /// <summary>
    /// Grid tile indices of the mother kudzu. Null until initialized.
    /// </summary>
    [DataField]
    public Vector2i? OriginIndices;

    [DataField]
    public EntityUid? OriginGrid;

    /// <summary>
    /// Chebyshev distance from the mother tile (2–3).
    /// </summary>
    [DataField]
    public int MaxRadius = 3;

    /// <summary>
    /// If set, spreads this prototype instead of itself (used by mother seeds).
    /// </summary>
    [DataField]
    public EntProtoId? SpreadAs;
}
