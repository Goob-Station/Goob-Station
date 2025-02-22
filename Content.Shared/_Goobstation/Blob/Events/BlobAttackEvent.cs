using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Blob;


[Serializable, NetSerializable]
public sealed class BlobAttackEvent : EntityEventArgs
{
    public readonly Vector2 Position;
    public readonly NetEntity BlobEntity;
    public readonly NetEntity AttackedEntity;

    public BlobAttackEvent(NetEntity blobEntity, NetEntity attackedEntity, Vector2 position)
    {
        Position = position;
        BlobEntity = blobEntity;
        AttackedEntity = attackedEntity;
    }
}
