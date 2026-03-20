using Content.Server.EUI;
using Content.Shared.Eui;
using Robust.Shared.Network;

using Content.Server.Silicons.StationAi;
using Content.Goobstation.Shared.Silicons;

namespace Content.Goobstation.Server.Silicons;

public sealed class StationAiEarlyLeaveEui : BaseEui
{
    private readonly NetUserId _userId;
    private readonly EntityUid _ai;
    private readonly StationAiEarlyLeaveSystem _leaveSystem;

    public StationAiEarlyLeaveEui(EntityUid ai, NetUserId userId, StationAiEarlyLeaveSystem leaveSystem)
    {
        _ai = ai;
        _userId = userId;
        _leaveSystem = leaveSystem;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not StationAiEarlyLeaveMessage choice ||
            !choice.Confirmed)
        {
            Close();
            return;
        }

        _leaveSystem.EarlyLeave(_ai, _userId);

        Close();
    }
}
