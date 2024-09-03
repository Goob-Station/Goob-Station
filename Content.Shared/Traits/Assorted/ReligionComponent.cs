// Public Domain Code
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Used for the atheist trait. Systems will check for this component in order to determine if atheist interactions occur.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReligionComponent : Component
{
    /// <summary>
    /// The religion of the entity with this component
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public string? Religion = null;
}
