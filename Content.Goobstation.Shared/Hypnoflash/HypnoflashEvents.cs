using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hypnoflash;

/// <summary>
/// raised on the entities in the hypnoflash radius
/// </summary>
[Serializable, NetSerializable]
public sealed partial class HypnoflashedEvent : EntityEventArgs
{
    public HypnoflashedEvent() {}
    public NetEntity Flasher;

    public HypnoflashedEvent(NetEntity flasher)
    {
        Flasher = flasher;
    }
}

/// <summary>
/// raised on the entity that originally activated the hypnoflash
/// </summary>
[Serializable, NetSerializable]
public sealed partial class HypnoflashActivatedEvent : EntityEventArgs
{
    public HypnoflashActivatedEvent() {} // fun. (kys)
    public NetEntity FlashEntity;

    public HypnoflashActivatedEvent(NetEntity flashEntity)
    {
        FlashEntity = flashEntity;
    }
}
