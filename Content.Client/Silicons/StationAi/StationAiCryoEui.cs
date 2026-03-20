using Content.Client.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

using Content.Client.Silicons.StationAi;
using Content.Shared.Silicons.StationAi;

[UsedImplicitly]
public sealed class StationAiCryoEui : BaseEui
{
    private readonly StationAiCryoMenu _menu;

    public StationAiCryoEui()
    {
        _menu = new StationAiCryoMenu();

        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new StationAiCryoMessage(false));
            _menu.Close();
        };

        _menu.ConfirmButton.OnPressed += _ =>
        {
            SendMessage(new StationAiCryoMessage(true));
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

        SendMessage(new StationAiCryoMessage(false));
        _menu.Close();
    }

}
