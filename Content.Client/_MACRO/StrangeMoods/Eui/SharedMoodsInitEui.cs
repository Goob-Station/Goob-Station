using Content.Client.Eui;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Eui;

namespace Content.Client._MACRO.StrangeMoods.Eui;

public sealed class SharedMoodsInitEui : BaseEui
{
    private readonly SharedMoodsInitUi _sharedMoodsUi;

    public SharedMoodsInitEui()
    {
        _sharedMoodsUi = new SharedMoodsInitUi();
        _sharedMoodsUi.OnNameAccepted += AcceptName;
    }

    private void AcceptName(string name)
    {
        SendMessage(new SharedMoodsInitAcceptMessage(name));
    }

    public override void Opened()
    {
        _sharedMoodsUi.OpenCentered();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case SharedMoodsInitValidMessage:
                {
                    _sharedMoodsUi.Close();
                    break;
                }
            case SharedMoodsInitErrorMessage:
                {
                    _sharedMoodsUi.ShowError();
                    break;
                }
        }
    }
}