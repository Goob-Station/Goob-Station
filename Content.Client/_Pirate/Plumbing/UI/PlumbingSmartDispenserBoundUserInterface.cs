using Content.Shared._Pirate.Plumbing;
using Content.Shared.Chemistry;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Pirate.Plumbing.UI;

[UsedImplicitly]
public sealed class PlumbingSmartDispenserBoundUserInterface : BoundUserInterface
{
    private PlumbingSmartDispenserWindow? _window;

    public PlumbingSmartDispenserBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<PlumbingSmartDispenserWindow>();

        _window.AmountGrid.OnButtonPressed += value => SendMessage(new PlumbingSmartDispenserSetDispenseAmountMessage(value));
        _window.ClearButton.OnPressed += _ => SendMessage(new ReagentDispenserClearContainerSolutionMessage());
        _window.OnDispenseReagentPressed += reagentId => SendMessage(new PlumbingSmartDispenserDispenseReagentMessage(reagentId));
        SendMessage(new PlumbingSmartDispenserRequestActorStateMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not PlumbingSmartDispenserBuiState cast)
            return;

        _window.UpdateSharedState(cast);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if (message is PlumbingSmartDispenserActorStateMessage actorState)
            _window?.UpdateActorState(actorState);
    }
}
