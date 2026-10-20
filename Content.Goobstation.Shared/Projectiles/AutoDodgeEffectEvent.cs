using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Projectiles;

/// <summary>
/// Sent to nearby clients when an entity auto-dodges so they play the dodge visuals.
/// </summary>
[Serializable, NetSerializable]
public sealed class AutoDodgeEffectEvent(NetEntity entity, Vector2 direction) : EntityEventArgs
{
    public NetEntity Entity = entity;

    public Vector2 Direction = direction;
}
