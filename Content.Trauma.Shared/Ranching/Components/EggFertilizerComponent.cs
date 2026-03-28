using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EggFertilizerComponent : Component
{
    [DataField]
    public float DoAfter = 15f;
}
