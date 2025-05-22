using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public partial class WoundSystem
{
    #region Data

    private readonly Dictionary<WoundSeverity, FixedPoint2> _woundThresholds = new()
    {
        { WoundSeverity.Healed, 0 },
        { WoundSeverity.Minor, 1 },
        { WoundSeverity.Moderate, 25 },
        { WoundSeverity.Severe, 50 },
        { WoundSeverity.Critical, 80 },
        { WoundSeverity.Loss, 100 },
    };

    #endregion
}
