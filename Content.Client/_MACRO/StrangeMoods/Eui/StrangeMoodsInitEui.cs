using Content.Client.Eui;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Eui;

namespace Content.Client._MACRO.StrangeMoods.Eui;

public sealed class StrangeMoodsInitEui : BaseEui
{
    private readonly StrangeMoodInitUi _strangeMoodUi;
    private NetEntity _target;

    public StrangeMoodsInitEui()
    {
        _strangeMoodUi = new StrangeMoodInitUi();
        _strangeMoodUi.OnPresetAccepted += AcceptPreset;
    }

    private void AcceptPreset(StrangeMoodDefinition def)
    {
        SendMessage(new StrangeMoodsInitAcceptMessage(def, _target));
        _strangeMoodUi.Close();
    }

    public override void Opened()
    {
        _strangeMoodUi.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not StrangeMoodsInitEuiState s)
            return;

        _target = s.Target;
    }
}