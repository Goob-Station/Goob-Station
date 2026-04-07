using Robust.Shared.Audio;
using Robust.Shared.GameStates;
namespace Content.Goobstation.Shared.Keypad;

[RegisterComponent, NetworkedComponent]
public sealed partial class KeypadComponent : Component
{
    [DataField]
    public string Code = string.Empty;

    [DataField]
    public int MaxLength = 4;

    [ViewVariables]
    public string Entered = string.Empty;

    /// <summary>
    /// If set, this keypad's code will be randomized on map init and shared
    /// with any KeypadCodePaperComponent that has the same KeypadGroup value.
    /// </summary>
    [DataField]
    public string? KeypadGroup = null;

    [DataField]
    public SoundSpecifier KeypadPressSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField]
    public SoundSpecifier AccessGrantedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/confirm_beep.ogg");

    [DataField]
    public SoundSpecifier AccessDeniedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

    [DataField]
    public SoundSpecifier ClearSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");
}
