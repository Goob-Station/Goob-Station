using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Aggression;

/// <summary>
/// Raised on the entity with AggressiveComponent when it added new aggressor.
/// </summary>
[Serializable, NetSerializable]
public sealed class AggressorAddedEvent : EntityEventArgs
{
    [DataField] public NetEntity Aggressor;

    public AggressorAddedEvent(NetEntity added)
    {
        Aggressor = added;
    }
}

/// <summary>
/// Raised on the entity with AggressiveComponent when it removed one of it's aggressors.
/// </summary>
[Serializable, NetSerializable]
public sealed class AggressorRemovedEvent : EntityEventArgs
{
    [DataField] public NetEntity Aggressor;

    public AggressorRemovedEvent(NetEntity removed)
    {
        Aggressor = removed;
    }
}
