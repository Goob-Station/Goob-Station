using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Hypnoflash;

/// <summary>
/// raised on the entities in the hypnoflash radius
/// </summary>
public sealed class HypnoflashedEvent : EntityEventArgs
{
    public HypnoflashedEvent() {}
    public EntityUid Flasher;

    public HypnoflashedEvent(EntityUid flasher)
    {
        Flasher = flasher;
    }
}

/// <summary>
/// raised on the entity that originally activated the hypnoflash
/// </summary>
public sealed class HypnoflashActivatedEvent : EntityEventArgs
{
    public HypnoflashActivatedEvent() {} // fun. (kys)
    public EntityUid FlashEntity;

    public HypnoflashActivatedEvent(EntityUid flashEntity)
    {
        FlashEntity = flashEntity;
    }
}
