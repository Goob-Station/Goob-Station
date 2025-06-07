using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Sandevistan;

[Serializable, NetSerializable]
public enum SandevistanState : byte
{
    Warning = 0,
    Shaking = 1,
    Stamina = 2,
    Damage = 3,
    Knockdown = 4,
    Disable = 5, // Sound is not looped to if you want to make Adam Smasher just keep this as the only threshold at like 60-70 and give him high LoadPerInactiveSecond. Or just set LoadPerActiveSecond to 0 if sound is not a problem.
    Death = 6,
}
