using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Keypad;

/// <summary>
/// Place it on a paper to automatically fill it with a code that matches keypadgroup of keypad entity.
/// </summary>
[RegisterComponent]
public sealed partial class KeypadCodePaperComponent : Component
{
    /// <summary>
    /// The group this belongs to. Pair it with a Keypad entity.
    /// </summary>
    [DataField(required: true)]
    public string KeypadGroup = string.Empty;

    /// <summary>
    /// Used to make more dynamic keycode papers. Must contain a {$code} substitution in the locale.
    /// </summary>
    [DataField]
    public LocId ContentLocId = "keypad-code-paper-content";
}
