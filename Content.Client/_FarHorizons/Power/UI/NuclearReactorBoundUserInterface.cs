using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

namespace Content.Client._FarHorizons.Power.UI;

/// <summary>
/// Initializes a <see cref="NuclearReactorWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class NuclearReactorBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private NuclearReactorWindow? _window;

    public NuclearReactorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<NuclearReactorWindow>();
        _window.SetEntity(Owner);

        _window.ItemActionButtonPressed += OnActionButtonPressed;
        _window.EjectButtonPressed += OnEjectButtonPressed;
        _window.ControlRodModify += OnControlRodModify;

        Update();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NuclearReactorBuiState reactorState)
            return;

        _window?.Update(reactorState);
    }

    private void OnActionButtonPressed(Vector2d vector)
    {
        if (_window is null ) return;

        SendPredictedMessage(new ReactorItemActionMessage(vector));
    }

    private void OnEjectButtonPressed()
    {
        if (_window is null) return;

        SendPredictedMessage(new ReactorEjectItemMessage());
    }

    private void OnControlRodModify(float amount)
    {
        if (_window is null) return;

        SendPredictedMessage(new ReactorControlRodModifyMessage(amount));
    }
}