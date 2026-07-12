using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Avali.Events;

/// <summary>
/// The stasis animation to play.
/// </summary>
[Serializable, NetSerializable]
public enum StasisAnimationType
{
    Prepare,
    Enter,
    Exit,
}

/// <summary>
/// Network event for a stasis animation.
/// </summary>
[Serializable, NetSerializable]
public sealed class StasisAnimationEvent : EntityEventArgs
{
    public NetEntity Entity;
    public NetCoordinates Coordinates;
    public StasisAnimationType AnimationType;

    public StasisAnimationEvent(NetEntity entity, NetCoordinates coordinates, StasisAnimationType animationType)
    {
        Entity = entity;
        Coordinates = coordinates;
        AnimationType = animationType;
    }
}
