using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Keypad;

/// <summary>
///     When placed on a paper entity, automatically populates it with the code
///     for whichever KeypadComponent shares the same <see cref="KeypadGroup"/> tag.
///     Both the paper and the keypad must have matching KeypadGroup values in YAML.
/// </summary>
[RegisterComponent]
public sealed partial class KeypadCodePaperComponent : Component
{
    /// <summary>
    ///     Arbitrary string that links this paper to a keypad.
    ///     Set the same value on a KeypadComponent to pair them.
    ///     Example: "dungeon_vault_door"
    /// </summary>
    [DataField(required: true)]
    public string KeypadGroup = string.Empty;
}
