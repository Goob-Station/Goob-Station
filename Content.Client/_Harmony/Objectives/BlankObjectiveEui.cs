using Content.Client.Eui;
using Content.Client._Harmony.Objectives;
using Content.Shared.Eui;
using Content.Shared.Objectives;

namespace Content.Server.Objectives;

public sealed class BlankObjectiveEui : BaseEui
{
    private BlankObjectiveUi _blankObjectiveUi;
    public BlankObjectiveEui()
    {
        _blankObjectiveUi = new BlankObjectiveUi();
        _blankObjectiveUi.OnClose += () => CloseWindow();
        _blankObjectiveUi.SubmitButton.OnPressed += _ => OnSubmitButtonPressed();
    }

    private void OnSubmitButtonPressed()
    {
        SendMessage(new ObjetiveSaveMessage(_blankObjectiveUi.GetObjectiveName(), _blankObjectiveUi.GetObjectiveDesc()));
        CloseWindow();
    }

    private void CloseWindow()
    {
        SendMessage(new CloseEuiMessage());
        _blankObjectiveUi.Close();
    }

    public override void Opened()
    {
        _blankObjectiveUi.OpenCentered();
    }
}
