using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Goobstation.Shared.Photo;

namespace Content.Goobstation.Client.Photo.Ui;

[UsedImplicitly]
public sealed class PhotoBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PhotoWindow? _window;

    public PhotoBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PhotoWindow>();
        _window.ScreenshotTaken += () => SendMessage(new UnsubscribePhotoVieverMessage());
        _window.LabelChanged += args =>
        {
            _window.SetLabel(args);
            SendMessage(new PhotoLabelledMessage() { NewLabel = args });
        };
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if (message is PhotoUiOpenedMessage cast)
        {
            _window?.Populate(cast.Map, cast.Offset);
            _window?.SetLabel(cast.Label);
        }

        if (message is StartLabellingPhotoMessage)
            _window?.ToggleLabelling();
    }
}
