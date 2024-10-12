using Content.Client.Eui;
using Content.Shared._Goobstation.Administration;
using Content.Shared.Eui;

namespace Content.Client._Goobstation.Administation.UI.TimeTransferPanel;

public sealed class TimeTransferPanelEui : BaseEui
{
    public TimeTransferPanel TimeTransferPanel { get; }

    public TimeTransferPanelEui()
    {
        TimeTransferPanel = new TimeTransferPanel();
        TimeTransferPanel.OnTransferMessageSend += args => SendMessage(new TimeTransferEuiMessage(args.playerId, args.jobId, args.time));
    }

    public override void Opened()
    {
        TimeTransferPanel.OpenCentered();
    }

    public override void Closed()
    {
        TimeTransferPanel.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not TimeTransferPanelEuiState cast)
            return;

        TimeTransferPanel.PopulateJobs(cast.PlaytrackerRoles);
        TimeTransferPanel.UpdateFlag(cast.HasFlag);
    }
}
