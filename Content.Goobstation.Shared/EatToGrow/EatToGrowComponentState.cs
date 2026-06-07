using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.EatToGrow;

[Serializable, NetSerializable]
public sealed class EatToGrowComponentState : ComponentState
{
    public readonly float Growth;
    public readonly float MaxGrowth;

    public readonly float CurrentScale;
    public readonly bool ShrinkOnDeath;
    public readonly int TimesGrown;
    public EatToGrowComponentState(float growth, float maxGrowth, float currentScale, bool shrinkOnDeath, int timesGrown)
    {
        Growth = growth;
        MaxGrowth = maxGrowth;
        CurrentScale = currentScale;
        ShrinkOnDeath = shrinkOnDeath;
        TimesGrown = timesGrown;
    }
}
