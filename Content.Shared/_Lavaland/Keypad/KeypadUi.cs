using Robust.Shared.Serialization;
namespace Content.Shared._Lavaland.Keypad;

[Serializable, NetSerializable]
public enum KeypadUiKey : byte { Key }

[Serializable, NetSerializable]
public sealed class KeypadUiState : BoundUserInterfaceState
{
    public int EnteredLength;
    public int MaxLength;
    public bool SuccessFlash;
}
