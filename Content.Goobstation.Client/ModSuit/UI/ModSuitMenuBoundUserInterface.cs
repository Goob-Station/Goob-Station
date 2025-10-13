using System.Linq;
using Content.Goobstation.Shared.ModSuits;
using Content.Shared.IdentityManagement;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.ModSuit.UI;

public sealed class ModSuitMenuBoundUserInterface : BoundUserInterface
{
    private ModSuitMenu? _menu;

    public ModSuitMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ModBoundUiState msg)
            return;

        if (EntMan.TryGetComponent<ModSuitComponent>(Owner, out var mod))
            _menu?.UpdateModStats(mod);

        _menu?.UpdateModuleView(msg);

    }
    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ModSuitMenu>();
        _menu.SetEntity(Owner);

        _menu.OpenCentered();

        if (EntMan.TryGetComponent<ModSuitComponent>(Owner, out var mod))
            _menu.UpdateModStats(mod);

        _menu.LockButton.OnPressed += _ => OnLockPressed();

        _menu.OnRemoveButtonPressed += owner => SendPredictedMessage(new ModModuleRemoveMessage(EntMan.GetNetEntity(owner)));
        _menu.OnActivateButtonPressed += owner => SendPredictedMessage(new ModModuleActivateMessage(EntMan.GetNetEntity(owner)));
        _menu.OnDeactivateButtonPressed += owner => SendPredictedMessage(new ModModuleDeactivateMessage(EntMan.GetNetEntity(owner)));
    }
    private void OnLockPressed()
    {
        var msg = new ModLockMessage(EntMan.GetNetEntity(Owner));
        SendPredictedMessage(msg);

        if (EntMan.TryGetComponent<ModSuitComponent>(Owner, out var mod))
            _menu?.UpdateModStats(mod);
    }
}
