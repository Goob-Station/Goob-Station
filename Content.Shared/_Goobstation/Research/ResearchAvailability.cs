using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Research;

[Serializable, NetSerializable]
public enum ResearchAvailability : byte
{
    Researched,
    Available,
    PrereqsMet,
    Unavailable
}
