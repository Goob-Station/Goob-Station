using Content.Client.Eui;
using Content.Shared.Ghost;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Devil.UI;

[UsedImplicitly]
public sealed class RevivalContractEui : BaseEui
{
    private readonly RevivalContractMenu _menu;

    public RevivalContractEui()
    {
        _menu = new RevivalContractMenu();

        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new ReturnToBodyMessage(false));
            _menu.Close();
        };

        _menu.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new ReturnToBodyMessage(true));
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

        SendMessage(new ReturnToBodyMessage(false));
        _menu.Close();
    }

}
