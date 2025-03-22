using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MadCarvingComponent : Component
{
    [DataField]
    public float StaminaDamage = 80f;

    [DataField]
    public TimeSpan MuteTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan BlindnessTime = TimeSpan.FromSeconds(10);
}
