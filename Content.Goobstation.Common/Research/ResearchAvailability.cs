using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Research;

[Serializable, NetSerializable]
public enum ResearchAvailability : byte
{
    Researched,
    Available,
    PrereqsMet,
    Unavailable
}
