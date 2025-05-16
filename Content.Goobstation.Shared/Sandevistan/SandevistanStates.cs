using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Sandevistan;

[Serializable, NetSerializable]
public enum SandevistanState : byte
{
    Normal = 0,
    Warning = 1,
    Shaking = 2,
    Pain = 3,
    Damage = 4,
    Stun = 5,
    Stamina = 6, // Not used but I'll leave this for yaml warriors
}
