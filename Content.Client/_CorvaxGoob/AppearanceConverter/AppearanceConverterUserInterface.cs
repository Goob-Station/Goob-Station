using Content.Shared._CorvaxGoob.AppearanceConverter;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CorvaxGoob.AppearanceConverter;

[UsedImplicitly]
public sealed class AppearanceConverterUserInterface : BoundUserInterface
{
    [ViewVariables]
    private AppearanceConverterWindow? _window;
    public AppearanceConverterUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<AppearanceConverterWindow>();
        _window.SetOwner(this.Owner);

        _window.ScanButton.OnPressed += _ => OnDNADataInput(_window.InputLineEdit.Text);
        _window.TransformButton.OnPressed += _ => OnTransformButtonPressed();
        _window.DeTransformButton.OnPressed += _ => OnDeTransformButtonPressed();

        _window.OnProfileSelected += dna => OnProfileSelect(dna);

        _window.OpenCentered();
    }

    private void OnDNADataInput(string input)
    {
        if (_window is null)
            return;

        if (input == "")
            return;

        SendMessage(new AppearanceConverterDNAScanDataMessage(input));
    }

    private void OnProfileSelect(string input)
    {
        if (_window is null)
            return;

        if (State is AppearanceConverterBoundUserInterfaceState)
            ((AppearanceConverterBoundUserInterfaceState) State).SelectedProfile = input;

        SendMessage(new AppearanceConverterSelectProfileMessage(input));
    }

    private void OnTransformButtonPressed()
    {
        if (_window is null)
            return;

        SendMessage(new AppearanceConverterTransformMessage());
    }

    private void OnDeTransformButtonPressed()
    {
        if (_window is null)
            return;

        SendMessage(new AppearanceConverterDeTransformMessage());
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not AppearanceConverterBoundUserInterfaceState)
            return;

        _window?.UpdateState((AppearanceConverterBoundUserInterfaceState) state);
    }
}
