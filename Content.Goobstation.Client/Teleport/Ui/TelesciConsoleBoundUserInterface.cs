
using System.Numerics;
using Content.Goobstation.Shared.Teleportation.Components;

namespace Content.Goobstation.Client.Teleport.Ui;

/// <summary>
/// This handles...
/// </summary>
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
        base.Open();
        if (_entMan.TryGetComponent<TelesciComputerComponent>(Owner, out var computer))
            _window = new TeleSciWindow(Owner, computer);
        else
            _window = new TeleSciWindow();

        _window.OnClose += Close;
        _window.OpenCentered();

        _window.OnSendButtonPressed += SendButtonPresed;
        _window.OnRetrieveButtonPressed += RetrieveButtonPresed;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _window?.Dispose();
    }

    // protected override void UpdateState(BoundUserInterfaceState state) => _window?.Update();

    private void SendButtonPresed(Vector2 location)
        =>  SendMessage(new TelesciSendMessage(location));

    private void RetrieveButtonPresed(Vector2 location)
        =>  SendMessage(new TelesciRetriveMessage(location));

    public void Update(Entity<TelesciComputerComponent> ent)
        =>   _window?.Update(ent);

}
