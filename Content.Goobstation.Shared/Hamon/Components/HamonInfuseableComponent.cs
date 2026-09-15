using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hamon.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HamonInfuseableComponent : Component
{
    [DataField]
    public TimeSpan InfuseTime = TimeSpan.FromSeconds(30);
}
