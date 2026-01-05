using Robust.Shared.Serialization;

namespace Content.Shared._Mono.ShipGuns;

/// <summary>
/// Component for categorizing ship guns by type
/// </summary>
[RegisterComponent]
public sealed partial class ShipGunTypeComponent : Component
{
    /// <summary>
    /// The type of ship gun
    /// </summary>
    [DataField("shipType")]
    public ShipGunType Type = ShipGunType.Ballistic;
}

/// <summary>
/// Types of ship guns
/// </summary>
[Serializable, NetSerializable]
public enum ShipGunType
{
    Ballistic,
    Energy,
    Missile,
    Mining //Lua Addition
}
