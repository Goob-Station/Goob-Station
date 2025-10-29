using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitcode.Heretic.Curses;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CurseData(NetEntity ent, string name, float multiplier, TimeSpan nextCurseTime)
{
    public CurseData() : this(NetEntity.Invalid, "", 0f, TimeSpan.Zero) { }

    [DataField]
    public NetEntity Entity = ent;

    [DataField]
    public string Name = name;

    [DataField]
    public float Multiplier = multiplier;

    [DataField]
    public TimeSpan NextCurseTime = nextCurseTime;
}

[Serializable, NetSerializable]
public sealed class PickCurseVictimState(HashSet<CurseData> data) : BoundUserInterfaceState
{
    public HashSet<CurseData> Data = data;
}

[Serializable, NetSerializable]
public sealed class CurseSelectedMessage(NetEntity ent, EntProtoId curse) : BoundUserInterfaceMessage
{
    public NetEntity Victim = ent;

    public EntProtoId Curse = curse;
}

[Serializable, NetSerializable]
public enum HereticCurseUiKey : byte
{
    Key
}
