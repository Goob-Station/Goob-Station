namespace Content.Goobstation.Common.Cyberdeck.Components;

/// <summary>
/// When Cyberdeck hacks this device, it will take
/// specified amount of charges.
/// </summary>
[RegisterComponent]
public sealed partial class CyberdeckHackableComponent : Component
{
    [DataField]
    public int Cost = 1;

    [DataField]
    public TimeSpan HackingTime = TimeSpan.FromSeconds(3);
}
