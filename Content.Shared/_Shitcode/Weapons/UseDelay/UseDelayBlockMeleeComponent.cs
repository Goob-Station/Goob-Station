using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.UseDelay;

[RegisterComponent, NetworkedComponent]
public sealed partial class UseDelayBlockMeleeComponent : Component
{
    [DataField]
    public List<string> Delays = new(){"default"};
}
