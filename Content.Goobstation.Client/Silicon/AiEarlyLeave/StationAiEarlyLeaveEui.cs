using Content.Client.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

using Content.Goobstation.Shared.Silicons;

namespace Content.Goobstation.Client.Silicon;

[UsedImplicitly]
public sealed class StationAiEarlyLeaveEui : BaseEui
{
    private readonly StationAiEarlyLeaveMenu _menu;

    public StationAiEarlyLeaveEui()
    {
        _menu = new StationAiEarlyLeaveMenu();

        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new StationAiEarlyLeaveMessage(false));
            _menu.Close();
        };

        _menu.ConfirmButton.OnPressed += _ =>
        {
            SendMessage(new StationAiEarlyLeaveMessage(true));
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

        SendMessage(new StationAiEarlyLeaveMessage(false));
        _menu.Close();
    }

}
