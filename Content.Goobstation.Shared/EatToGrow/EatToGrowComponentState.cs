using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.EatToGrow;

[Serializable, NetSerializable]
public sealed class EatToGrowComponentState : ComponentState
{
    public readonly float Growth;
    public readonly float MaxGrowth;
    public readonly float CurrentScale;

    public EatToGrowComponentState(float growth, float maxGrowth, float currentScale)
    {
        Growth = growth;
        MaxGrowth = maxGrowth;
        CurrentScale = currentScale;
    }
}
