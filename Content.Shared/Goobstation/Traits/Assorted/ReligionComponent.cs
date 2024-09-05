// Public Domain Code
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Used for religions. Systems will check for this component and its fields in order to determine if religion based interactions occur.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReligionComponent : Component
{
    /// <summary>
    /// The religion of the entity
    /// </summary>
    [DataField("religion")]
    [AutoNetworkedField]
    public string? Religion = null;

    /// <summary>
    /// boolean of if the entity is an atheist
    /// </summary>
    [DataField("isAtheist")]
    [AutoNetworkedField]
    public bool IsAtheist = false;
}
