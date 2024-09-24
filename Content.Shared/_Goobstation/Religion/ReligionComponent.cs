// Public Domain Code Begins

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Religion;

/// <summary>
///     Used for religions. Systems will check for this component and its fields in order to determine if religion based
///     interactions occur.
/// </summary>
[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState]
public sealed partial class ReligionComponent : Component
{
    /// <summary>
    ///     The religion of the entity
    /// </summary>
    [DataField("religion")] [AutoNetworkedField]
    public Religion Type = Religion.None;
}

public enum Religion
{
    None,
    Atheist,
    Buddhist,
    Christian,
}
// Public Domain Code Ends
