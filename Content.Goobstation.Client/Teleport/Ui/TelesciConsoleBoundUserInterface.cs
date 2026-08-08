
using System.Numerics;
using Content.Goobstation.Shared.Teleportation.Components;

namespace Content.Goobstation.Client.Teleport.Ui;

public sealed class TelesciConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    [ViewVariables]
    private TeleSciWindow? _window;

    public TelesciConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        if (!_entMan.TryGetComponent<TelesciComputerComponent>(Owner, out var computer))
            return;

        base.Open();

        _window = new TeleSciWindow(Owner, computer);

        _window.OnClose += Close;
        _window.OpenCentered();

        _window.OnSendButtonPressed += SendButtonPresed;
        _window.OnRetrieveButtonPressed += RetrieveButtonPresed;
        _window.OnPositionChange += PositionChanged;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _window?.Dispose();
    }


    private void SendButtonPresed(Vector2 location) => SendMessage(new TelesciSendMessage(location));
    private void RetrieveButtonPresed(Vector2 location) => SendMessage(new TelesciRetrieveMessage(location));
    private void PositionChanged(Vector2 position) => SendMessage(new TelesciPositionMessage(position));
}
