using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    #region Data

    private readonly Dictionary<BoneSeverity, FixedPoint2> _boneThresholds = new()
    {
        { BoneSeverity.Normal, 40 },
        { BoneSeverity.Damaged, 25 },
        { BoneSeverity.Cracked, 10 },
        { BoneSeverity.Broken, 0 },
    };

    private readonly Dictionary<BoneSeverity, FixedPoint2> _bonePainModifiers = new()
    {
        { BoneSeverity.Normal, 0.4 },
        { BoneSeverity.Damaged, 0.6 },
        { BoneSeverity.Cracked, 0.8 },
        { BoneSeverity.Broken, 1 },
    };

    private readonly Dictionary<WoundableSeverity, FixedPoint2> _boneTraumaChanceMultipliers = new()
    {
        { WoundableSeverity.Healthy, 0 },
        { WoundableSeverity.Minor, 0.01 },
        { WoundableSeverity.Moderate, 0.04 },
        { WoundableSeverity.Severe, 0.12 },
        { WoundableSeverity.Critical, 0.21 },
        { WoundableSeverity.Loss, 0.21 },
    };

    private readonly Dictionary<WoundableSeverity, FixedPoint2> _boneDamageMultipliers = new()
    {
        { WoundableSeverity.Healthy, 0 },
        { WoundableSeverity.Minor, 0.4 },
        { WoundableSeverity.Moderate, 0.6 },
        { WoundableSeverity.Severe, 0.9 },
        { WoundableSeverity.Critical, 1.25 },
        { WoundableSeverity.Loss, 1.6 }, // Fun.
    };

    #endregion
}
