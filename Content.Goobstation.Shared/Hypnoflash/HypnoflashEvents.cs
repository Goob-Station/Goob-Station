using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hypnoflash;

/// <summary>
/// raised on the entities in the hypnoflash radius
/// </summary>
[Serializable, NetSerializable]
public readonly record struct HypnoflashedEvent;

/// <summary>
/// raised on the entity that originally activated the hypnoflash
/// </summary>
[Serializable, NetSerializable]
public readonly record struct HypnoflashActivatedEvent;
