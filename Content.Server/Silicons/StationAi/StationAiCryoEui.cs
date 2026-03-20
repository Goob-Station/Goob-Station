using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Network;

namespace Content.Server.Silicons.StationAi;

public sealed class StationAiCryoEui : BaseEui
{
    private readonly NetUserId _userId;
    private readonly EntityUid _ai;
    private readonly StationAiSystem _aiSystem;

    public StationAiCryoEui(EntityUid ai, NetUserId userId, StationAiSystem stationAi)
    {
        _ai = ai;
        _userId = userId;
        _aiSystem = stationAi;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not StationAiCryoMessage choice ||
            !choice.Confirmed)
        {
            Close();
            return;
        }

        _aiSystem.CryoAi(_ai, _userId);

        Close();
    }
}
