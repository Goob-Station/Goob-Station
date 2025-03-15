using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Research;

[Serializable, NetSerializable]
public enum ResearchAvailablity : byte
{
    Researched,
    Available,
    Unavailable
}
