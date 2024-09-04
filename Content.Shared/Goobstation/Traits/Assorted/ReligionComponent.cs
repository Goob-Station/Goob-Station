// Public Domain Code
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Used for religions. Systems will check for this component and its field in order to determine if religion based interactions occur.
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
