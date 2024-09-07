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
    public Religion? Type = null;
}
public enum Religion
{
    Christian,
    Islamic,
    Atheist,
    Hindu,
    Buddhist,
    None
};
