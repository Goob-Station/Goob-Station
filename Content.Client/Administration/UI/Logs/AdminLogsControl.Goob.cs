using Content.Client.Administration.UI.CustomControls;
using Content.Goobstation.Shared.Paint;
using Content.Shared.Database;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.Logs;

public sealed partial class AdminLogsControl
{
    private void GoobAddLogControls(AdminLogLabel label)
    {
        if (label.Log.Type != LogType.Paint || !PaintLogToken.TryFind(label.Log.Message, out var token))
            return;

        var round = SelectedRoundId;
        var button = new Button
        {
            Text = Loc.GetString("paint-log-view-button"),
            HorizontalAlignment = HAlignment.Left,
            Visible = label.Visible,
        };

        button.OnPressed += _ => IoCManager.Resolve<IEntityManager>()
            .EntityNetManager
            .SendSystemNetworkMessage(new PaintLogImageRequestEvent(round, token));
        label.OnVisibilityChanged += control => button.Visible = control.Visible;
        LogsContainer.AddChild(button);
    }
}
