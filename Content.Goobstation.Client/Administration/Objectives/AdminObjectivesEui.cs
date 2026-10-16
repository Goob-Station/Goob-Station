using Content.Client.Eui;
using Content.Goobstation.Shared.Administration.Objectives;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Goobstation.Client.Administration.Objectives;

[UsedImplicitly]
public sealed class AdminObjectivesEui : BaseEui
{
    private readonly AdminObjectivesWindow _window;

    public AdminObjectivesEui()
    {
        _window = new AdminObjectivesWindow();
        _window.OnClose += () => SendMessage(new CloseEuiMessage());

        _window.OnAddPrototype += (proto, target, bypass) =>
            SendMessage(new AdminObjectivesAddPrototypeMessage(proto, target, bypass));
        _window.OnAddCustom += (title, description, issuer) =>
            SendMessage(new AdminObjectivesAddCustomMessage(title, description, issuer));
        _window.OnAddSteal += (item, issuer) =>
            SendMessage(new AdminObjectivesAddStealMessage(item, issuer));
        _window.OnRemove += objective =>
            SendMessage(new AdminObjectivesRemoveMessage(objective));
        _window.OnRefresh += () =>
            SendMessage(new AdminObjectivesRefreshMessage());
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not AdminObjectivesEuiState cast)
            return;

        _window.UpdateState(cast);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not AdminObjectivesResultMessage result)
            return;

        _window.HandleResult(result.Message, result.Success);
    }
}
