using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.EatToGrow;


[RegisterComponent, NetworkedComponent]


public sealed partial class EatToGrowComponent : Component
{
    [DataField("growth")]
    public float Growth = 0.1f; // percentage growth

    [DataField("maxGrowth")]
    public float MaxGrowth = 5.0f; // max allowed scale multiplier

    [DataField("currentScale")]
    public float CurrentScale = 1.0f; // current scale
}
