using Robust.Shared.Serialization;
namespace Content.Shared._Lavaland.Keypad;

[Serializable, NetSerializable]
public sealed class KeypadDigitMessage : BoundUserInterfaceMessage
{
    public int Digit;
    public KeypadDigitMessage(int digit) { Digit = digit; }
}

[Serializable, NetSerializable]
public sealed class KeypadClearMessage : BoundUserInterfaceMessage { }

[Serializable, NetSerializable]
public sealed class KeypadEnterMessage : BoundUserInterfaceMessage { }
