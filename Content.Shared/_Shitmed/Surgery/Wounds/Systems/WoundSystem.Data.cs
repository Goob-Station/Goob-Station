using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public partial class WoundSystem
{
    #region Data

    private static readonly KeyValuePair<WoundSeverity, FixedPoint2>[] WoundThresholds =
    [
        new(WoundSeverity.Loss, 100),
        new(WoundSeverity.Critical, 80),
        new(WoundSeverity.Severe, 50),
        new(WoundSeverity.Moderate, 25),
        new(WoundSeverity.Minor, 1),
        new(WoundSeverity.Healed, 0),
    ];

    #endregion
}
