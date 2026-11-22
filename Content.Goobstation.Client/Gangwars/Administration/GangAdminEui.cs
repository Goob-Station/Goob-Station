using Content.Client.Eui;
using Content.Goobstation.Shared.Gangwars.Administration;
using Content.Shared.Eui;

namespace Content.Goobstation.Client.Gangwars.Administration;

public sealed class GangAdminEui : BaseEui
{
    private readonly GangAdminWindow _window;

    public GangAdminEui()
    {
        _window = new GangAdminWindow();
        _window.OnManualRename += (oldColor, newColor, name) =>
            SendMessage(new GangAdminManualRenameMessage(oldColor, newColor, name));
        _window.OnForceRename += color =>
            SendMessage(new GangAdminForceRenameMessage(color));
        _window.OnKick += member =>
            SendMessage(new GangAdminKickMessage(member));
        _window.OnSetLeader += member =>
            SendMessage(new GangAdminSetLeaderMessage(member));
        _window.OnSetPoints += (member, points) =>
            SendMessage(new GangAdminSetPointsMessage(member, points));
        _window.OnAddMember += (color, player) =>
            SendMessage(new GangAdminAddMemberMessage(color, player));
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not GangAdminEuiState cast)
            return;

        _window.SetData(cast.Gangs, cast.Players);
    }
}
