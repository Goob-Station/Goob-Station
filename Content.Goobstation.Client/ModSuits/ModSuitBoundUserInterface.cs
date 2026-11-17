using Content.Goobstation.Shared.ModSuits;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.ModSuits;

public sealed class ModSuitBoundUserInterface : BoundUserInterface
{
    private ModSuitWindow? _window;

    public ModSuitBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ModSuitWindow>();
        _window.SetSuit(Owner);
        _window.OnActivate += () => SendPredictedMessage(new ModSuitActivateMessage());
        _window.OnToggleAllParts += () => SendPredictedMessage(new ModSuitToggleAllPartsMessage());
        _window.OnTogglePart += slot => SendPredictedMessage(new ModSuitTogglePartMessage(slot));
        _window.OnModuleToggle += module => SendPredictedMessage(new ModSuitModuleToggleMessage(module));
        _window.OnModulePin += module => SendPredictedMessage(new ModSuitModulePinMessage(module));
        _window.OnModuleRemove += module => SendPredictedMessage(new ModSuitModuleRemoveMessage(module));
    }

    public override void Update()
    {
        base.Update();
        _window?.Refresh();
    }
}
