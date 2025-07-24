using Content.Shared._CorvaxNext.MedipenRefiller;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CorvaxNext.MedipenRefiller;

[UsedImplicitly]
public sealed class MedipenRefillerUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MedipenRefillerWindow? _window;
    public MedipenRefillerUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MedipenRefillerWindow>();
        _window.SetOwner(Owner);

        _window.ApplySettingsButton.OnButtonDown += _ =>
        {
            if (int.TryParse(_window.MedipenDosage.Text, out var result))
                SendMessage(new MedipenRefillerApplySettingsMessage(_window.LabelLineEdit.Text,
                    _window.ColorPicker.Color, Math.Clamp(result, 0, _window.GetMaxVolume())));
        };

        _window.FillMedipenButton.OnButtonDown += _ =>
        {
            if (int.TryParse(_window.MedipenDosage.Text, out var result))
                SendMessage(new MedipenRefillerFillMedipenMessage(Math.Clamp(result, 0, _window.GetMaxVolume())));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        _window?.UpdateState((MedipenRefillerBoundUserInterfaceState)state);
    }
}
