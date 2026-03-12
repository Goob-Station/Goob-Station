namespace Content.Goobstation.Shared.Hypnoflash;

/// <summary>
/// raised on the entities in the hypnoflash radius
/// </summary>
[Serializable]
public sealed partial class HypnoflashedEvent : EntityEventArgs
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
[Serializable]
public sealed partial class HypnoflashActivatedEvent : EntityEventArgs
{
    public HypnoflashActivatedEvent() {} // fun. (kys)
    public EntityUid FlashEntity;

    public HypnoflashActivatedEvent(EntityUid flashEntity)
    {
        FlashEntity = flashEntity;
    }
}
