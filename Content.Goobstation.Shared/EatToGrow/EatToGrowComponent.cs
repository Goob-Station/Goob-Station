using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.EatToGrow;
[RegisterComponent, NetworkedComponent]
public sealed partial class EatToGrowComponent : Component
{
    [DataField]
    public float Growth = 0.1f; // percentage growth

    [DataField]
    public float MaxGrowth = 5.0f; // max allowed scale multiplier

    [DataField]
    public float CurrentScale = 1.0f; // current scale

    [DataField]
    public bool ShrinkOnDeath = true; // Revert to original size on death?

    [DataField]
    public int TimesGrown = 0; // how many times have they grown?
}
