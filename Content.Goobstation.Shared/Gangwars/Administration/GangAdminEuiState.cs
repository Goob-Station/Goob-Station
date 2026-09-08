using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gangwars.Administration;

[Serializable, NetSerializable]
public sealed class GangAdminEuiState(List<GangAdminInfo> gangs, List<GangAdminPlayerInfo> players) : EuiStateBase
{
    public List<GangAdminInfo> Gangs = gangs;
    public List<GangAdminPlayerInfo> Players = players;
}

[Serializable, NetSerializable]
public struct GangAdminPlayerInfo
{
    public NetEntity Entity;
    public string Name;
    public bool InGang;
}

[Serializable, NetSerializable]
public struct GangAdminInfo
{
    public Color Color;
    public string Name;
    public List<GangAdminMemberInfo> Members;
}

[Serializable, NetSerializable]
public struct GangAdminMemberInfo
{
    public NetEntity Entity;
    public string Name;
    public bool IsLeader;
    public int Points;
}

[Serializable, NetSerializable]
public sealed class GangAdminManualRenameMessage(Color oldColor, Color newColor, string newName) : EuiMessageBase
{
    public Color OldColor = oldColor;
    public Color NewColor = newColor;
    public string NewName = newName;
}

[Serializable, NetSerializable]
public sealed class GangAdminForceRenameMessage(Color color) : EuiMessageBase
{
    public Color Color = color;
}

[Serializable, NetSerializable]
public sealed class GangAdminKickMessage(NetEntity member) : EuiMessageBase
{
    public NetEntity Member = member;
}

[Serializable, NetSerializable]
public sealed class GangAdminSetLeaderMessage(NetEntity member) : EuiMessageBase
{
    public NetEntity Member = member;
}

[Serializable, NetSerializable]
public sealed class GangAdminAddMemberMessage(Color gangColor, NetEntity player) : EuiMessageBase
{
    public Color GangColor = gangColor;
    public NetEntity Player = player;
}

[Serializable, NetSerializable]
public sealed class GangAdminSetPointsMessage(NetEntity member, int points) : EuiMessageBase
{
    public NetEntity Member = member;
    public int Points = points;
}
