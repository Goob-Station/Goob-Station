using Content.Client.Eui;
using Content.Goobstation.Client.UserInterface;
using Content.Shared.Actions.Events;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Actions;

public sealed partial class ConfirmableActionEui : BaseEui
{
    private readonly SimpleConfirmationMenu _menu;

    public ConfirmableActionEui()
    {
        _menu = new SimpleConfirmationMenu("confirmable-action-desc", "confirmable-action-yes", "confirmable-action-no");

        _menu.ConfirmButton.OnPressed += _ =>
        {
            SendMessage(new ConfirmableActionEuiMessage(true));
            _menu.Close();
        };

        _menu.CancelButton.OnPressed += _ =>
        {
            SendMessage(new ConfirmableActionEuiMessage(false));
            _menu.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _menu.Close();
    }
}
