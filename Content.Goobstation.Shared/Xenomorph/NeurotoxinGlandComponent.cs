using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenomorph;

/// <summary>
/// Handles the acid spit gun action
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NeurotoxinGlandComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Active;
}
