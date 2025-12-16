using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

namespace Content.Client._FarHorizons.Power.UI;

/// <summary>
/// Initializes a <see cref="NuclearReactorWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class NuclearReactorMonitorBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    [ViewVariables]
    private NuclearReactorWindow? _window;

    public NuclearReactorMonitorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        // Check if opened by a reactor monitor
        if(!_entityManager.TryGetComponent<NuclearReactorMonitorComponent>(Owner, out var reactorMonitorComponent))
            return;

        // Check if reactor monitor has an attached entity
        if (!_entityManager.TryGetEntity(reactorMonitorComponent.reactor, out var reactor) || reactor == null)
            return;

        // Check if the attached entity is a nuclear reactor and that it's not melted
        if (!_entityManager.TryGetComponent<NuclearReactorComponent>(reactor, out var reactorComponent) || reactorComponent.Melted)
            return;

        base.Open();

        _window = this.CreateWindow<NuclearReactorWindow>();
        _window.SetEntity(reactor.Value, Owner);

        _window.ControlRodModify += OnControlRodModify;

        Update();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NuclearReactorBuiState reactorState)
            return;

        _window?.Update(reactorState);
    }

    private void OnControlRodModify(float amount)
    {
        if (_window is null) return;

        SendPredictedMessage(new ReactorControlRodModifyMessage(amount));
    }
}