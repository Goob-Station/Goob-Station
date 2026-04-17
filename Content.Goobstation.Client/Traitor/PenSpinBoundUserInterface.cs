using Content.Goobstation.Common.Traitor.PenSpin;
using Content.Goobstation.Shared.Traitor.PenSpin;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Traitor;

[UsedImplicitly]
public sealed class PenSpinBoundUserInterface : BoundUserInterface
{
    private PenSpinMenu? _menu;

    public PenSpinBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<PenSpinMenu>();
        _menu.OpenCentered();

        _menu.ResetButtonPressed += OnResetPressed;
        _menu.SubmitButtonPressed += OnSubmitPressed;

        if (EntMan.TryGetComponent<PenComponent>(Owner, out var comp))
        {
            _menu.SetDegreeRange(comp.MinDegree, comp.MaxDegree);
        }
    }

    private void OnResetPressed()
    {
        SendPredictedMessage(new PenSpinResetMessage());
    }

    private void OnSubmitPressed(int degree)
    {
        SendPredictedMessage(new PenSpinSubmitDegreeMessage(degree));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _menu?.Dispose();
        }
    }
}
