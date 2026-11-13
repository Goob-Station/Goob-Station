using Content.Goobstation.Shared.Materials.OreSilo; // Goob
using Content.Shared.Materials.OreSilo;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Materials.UI;

[UsedImplicitly]
public sealed class OreSiloBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private OreSiloMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<OreSiloMenu>();
        _menu.SetEntity(Owner);

        // Goob
        if (EntMan.TryGetComponent<OreSiloComponent>(Owner, out var silo) && silo.MasterSilo)
            _menu.SetMasterSilo(Owner);

        _menu.OnTransferPressed += (target, material, sheets) =>
        {
            SendMessage(new OreSiloTransferMessage(target, material, sheets));
        };
        // Goob

        _menu.OnClientEntryPressed += netEnt =>
        {
            SendPredictedMessage(new ToggleOreSiloClientMessage(netEnt));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not OreSiloBuiState msg)
            return;
        _menu?.Update(msg);
    }
}
