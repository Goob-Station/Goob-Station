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

    [DataField("keypadPressSound")]
    public SoundSpecifier KeypadPressSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField("accessGrantedSound")]
    public SoundSpecifier AccessGrantedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/confirm_beep.ogg");

    [DataField("accessDeniedSound")]
    public SoundSpecifier AccessDeniedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

    [DataField("clearSound")]
    public SoundSpecifier ClearSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");
}

[Serializable, NetSerializable]
public enum KeypadUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class KeypadUiState : BoundUserInterfaceState
{
    public int EnteredLength;
    public int MaxLength;
    public bool SuccessFlash;
}

[Serializable, NetSerializable]
public sealed class KeypadDigitMessage : BoundUserInterfaceMessage
{
    public int Digit;
    public KeypadDigitMessage(int digit)
    {
        Digit = digit;
    }
}

[Serializable, NetSerializable]
public sealed class KeypadClearMessage : BoundUserInterfaceMessage { }

[Serializable, NetSerializable]
public sealed class KeypadEnterMessage : BoundUserInterfaceMessage { }
