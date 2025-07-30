using Content.Pirate.Shared.Botany.PlantAnalyzer;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Pirate.Client.Botany.PlantAnalyzer;

[UsedImplicitly]
public sealed class PlantAnalyzerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PlantAnalyzerWindow? _window;

    public PlantAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<PlantAnalyzerWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _window.Print.OnPressed += _ => Print();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_window is null
            || message is not PlantAnalyzerScannedUserMessage cast)
            return;

        _window.Populate(cast);
    }

    private void Print()
    {
        SendMessage(new PlantAnalyzerPrintMessage());
        if (_window is null)
            return;

        _window.Print.Disabled = true;
    }
}
