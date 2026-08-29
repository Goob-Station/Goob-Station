// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    #region Data

    /// <summary>
    /// Sorted in descending order by threshold value.
    /// </summary>
    private static readonly KeyValuePair<BoneSeverity, FixedPoint2>[] BoneThresholds =
    [
        new(BoneSeverity.Normal, 40),
        new(BoneSeverity.Damaged, 25),
        new(BoneSeverity.Cracked, 10),
        new(BoneSeverity.Broken, 0),
    ];

    #endregion
}
