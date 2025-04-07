using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Actions;

[Serializable, NetSerializable]
public sealed class LoadActionsEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity = entity;
}
