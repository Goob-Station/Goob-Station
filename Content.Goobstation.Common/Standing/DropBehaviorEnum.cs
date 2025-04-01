using System;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Standing;



[Serializable, NetSerializable]
public enum DropHeldItemsBehavior : byte
{
    NoDrop,
    DropIfStanding,
    AlwaysDrop
}
