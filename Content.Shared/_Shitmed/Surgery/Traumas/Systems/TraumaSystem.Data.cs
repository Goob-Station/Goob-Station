// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Medical.Surgery.Wounds;
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

    private readonly Dictionary<WoundableSeverity, FixedPoint2> _boneTraumaChanceMultipliers = new()
    {
        { WoundableSeverity.Healthy, 0 },
        { WoundableSeverity.Minor, 0.01 },
        { WoundableSeverity.Moderate, 0.04 },
        { WoundableSeverity.Severe, 0.12 },
        { WoundableSeverity.Critical, 0.21 },
        { WoundableSeverity.Mangled, 0.21 },
        { WoundableSeverity.Severed, 0 },
    };

    #endregion
}
