using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Emoting;

[RegisterComponent, NetworkedComponent]
public sealed partial class CounterStatusEffectComponent : Component
{
    [DataField]
    public int Count;
}
