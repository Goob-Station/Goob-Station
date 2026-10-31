using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage;

public partial class DamageModifierSet
{
    /// <summary>
    /// Goobstation.
    /// Whether this modifier set will ignore incoming damage partial armor penetration, positive or negative.
    /// Used mainly for species modifier sets.
    /// </summary>
    [DataField(customTypeSerializer: typeof(FlagSerializer<ArmorPierceFlags>))]
    public int IgnoreArmorPierceFlags = (int) PartialArmorPierceFlags.None;
}
public sealed partial class ArmorPierceFlags;

[Flags, Serializable]
[FlagsFor(typeof(ArmorPierceFlags))]
public enum PartialArmorPierceFlags
{
    None = 0,
    Positive = 1 << 0,
    Negative = 1 << 1,
    All = Positive | Negative,
}