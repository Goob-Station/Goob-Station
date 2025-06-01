using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Shared.Cyberdeck.Components;

/// <summary>
/// When Cyberdeck hacks this device, it will take
/// specified amount of charges.
/// </summary>
[RegisterComponent]
public sealed partial class CyberdeckHackableComponent : Component
{
    [DataField]
    public FixedPoint2 Cost = 1;

    [DataField]
    public TimeSpan HackingTime = TimeSpan.FromSeconds(3);
}
