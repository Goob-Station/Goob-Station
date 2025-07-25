using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Werewolf.Components;

/// <summary>
/// This is the main component of the werewolf
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WerewolfComponent : Component
{
    /// <summary>
    /// The currency for buying werewolf forms.
    /// Can be gained by eating humanoid organs.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public int Fury;
}
