using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Traitor.PenSpin;

[RegisterComponent, NetworkedComponent]
public sealed partial class PenSpinComponent : Component
{
    [DataField]
    public int MinDegree = 0;

    [DataField]
    public int MaxDegree = 359;

    [DataField]
    public int CombinationLength = 4;

    [DataField]
    public TimeSpan SpinCooldown = TimeSpan.FromMilliseconds(300);
}
