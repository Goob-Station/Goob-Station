using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Cyberdeck.Components;

/// <summary>
/// When Cyberdeck hacks this device, it will take
/// specified amount of charges.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberdeckHackableComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Cost = 1;

    [DataField, AutoNetworkedField]
    public TimeSpan HackingTime = TimeSpan.FromSeconds(3);
}
