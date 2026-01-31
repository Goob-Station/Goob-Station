using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Power._FarHorizons.Power.UI;

/// <summary>
/// Initializes a <see cref="NuclearReactorWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class NuclearReactorBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    [ViewVariables]
    private NuclearReactorWindow? _window;

    public NuclearReactorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        EntityUid? reactorUid = null;
        if (_entityManager.TryGetComponent<NuclearReactorMonitorComponent>(Owner, out var reactorMonitorComponent))
        {
            if (!_entityManager.TryGetEntity(reactorMonitorComponent.reactor, out reactorUid) || reactorUid == null
                || !_entityManager.TryGetComponent<NuclearReactorComponent>(reactorUid, out var monitoredReactorComponent) || monitoredReactorComponent.Melted)
                return;
        }
        else if (!_entityManager.TryGetComponent<NuclearReactorComponent>(Owner, out var reactorComponent) || reactorComponent.Melted)
            return;

        base.Open();

        _window = this.CreateWindow<NuclearReactorWindow>();
        if (_entityManager.EntityExists(reactorUid))
            _window.SetEntity(reactorUid.Value, Owner);
        else
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
