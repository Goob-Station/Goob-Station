using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Keypad;

[RegisterComponent, NetworkedComponent]
public sealed partial class KeypadComponent : Component
{
    [DataField]
    public string Code;

    [DataField]
    public int MaxLength = 4;

    [ViewVariables]
    public string Entered = string.Empty;

    [DataField]
    public SoundSpecifier KeypadPressSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField]
    public SoundSpecifier AccessGrantedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/confirm_beep.ogg");

    [DataField]
    public SoundSpecifier AccessDeniedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

    [DataField]
    public SoundSpecifier ClearSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");
}
