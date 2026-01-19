using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Heretic;

[Serializable, NetSerializable]
public sealed class ButtonTagPressedEvent(string id, NetEntity user, NetCoordinates coords) : EntityEventArgs
{
    public NetEntity User = user;

    public NetCoordinates Coords = coords;

    public string Id = id;
}

[ByRefEvent]
public record struct HereticCheckEvent(EntityUid Uid, HereticCheckType Type, bool Result = false);

[Flags]
public enum HereticCheckType
{
    Heretic = 1 << 0,
    Ghoul = 1 << 1,
    Ascended = 1 << 2,
}
