using System;
using Content.Pirate.Shared.ReactionChamber.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Pirate.Client.ReactionChamber.UI;

[UsedImplicitly]
public sealed class ReactionChamberBoundUI : BoundUserInterface
{


    [ViewVariables]
    private ReactionChamberWindow _window;
    public ReactionChamberBoundUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _window = new ReactionChamberWindow();
        _window.FindControl<Button>("ActiveButton").OnPressed += _ => onActiveBtnPressed(!_window.Active);
        _window.FindControl<FloatSpinBox>("TempField").OnValueChanged += _ => onTempFieldEntered(_window.FindControl<FloatSpinBox>("TempField").Value);
    }
    protected override void Open()
    {
        base.Open();
        _window.OnClose += Close;

        if (State != null)
        {
            UpdateState(State);
        }

        _window.OpenCentered();
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        var castState = (ReactionChamberBoundUIState) state;
        _window.UpdateState(castState);
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Close();
    }
    private void onActiveBtnPressed(bool active)
    {
        _window.SetActive(active);
        SendMessage(new ReactionChamberActiveChangeMessage(active));
    }
    private void onTempFieldEntered(float temp)
    {
        _window.SetTemp(temp);
        SendMessage(new ReactionChamberTempChangeMessage(temp));
    }

}
