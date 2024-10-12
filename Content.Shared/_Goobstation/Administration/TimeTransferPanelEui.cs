using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Administration;

[Serializable, NetSerializable]
public sealed class TimeTransferPanelEuiState : EuiStateBase
{
    public List<string> PlaytrackerRoles { get; }

    public bool HasFlag { get; set; }

    public TimeTransferPanelEuiState(List<string> roles, bool hasFlag)
    {
        PlaytrackerRoles = roles;
        HasFlag = hasFlag;
    }
}

[Serializable, NetSerializable]
public sealed class TimeTransferEuiMessage : EuiMessageBase
{
    public NetUserId Player;
    public string Job;
    public float Time;

    public TimeTransferEuiMessage(NetUserId player, string job, float time)
    {
        Player = player;
        Job = job;
        Time = time;
    }
}
