using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Cybernetics;

/// <summary>
/// Component for cybernetic implants that can be installed in entities
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberneticsComponent : Component 
{
    /// <summary>
    ///     Is the cybernetic implant disabled by EMPs, etc?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Disabled = false;
}