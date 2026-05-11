using Robust.Shared.Serialization;

// Fuck the namespace fix this later
namespace Content.Shared.Movement.Systems;

[Flags]
[Serializable, NetSerializable]
public enum MoveButtons : byte
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 4,
    Right = 8,
    Walk = 16,
    AnyDirection = Up | Down | Left | Right,
}
