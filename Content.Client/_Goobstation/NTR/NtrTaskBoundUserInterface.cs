using Content.Client._Goobstation.NTR.UI;
using Content.Client.Cargo.UI;
using Content.Shared._Goobstation.NTR;
using Content.Shared.Cargo.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Goobstation.NTR;

[UsedImplicitly]
public sealed class NtrTaskBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NtrTaskMenu? _menu;

    public NtrTaskBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new();

        _menu.OnClose += Close;
        _menu.OpenCentered();

        _menu.OnLabelButtonPressed += id =>
        {
            SendMessage(new TaskPrintLabelMessage(id));
        };

        _menu.OnSkipButtonPressed += id =>
        {
            SendMessage(new TaskSkipMessage(id));
        };
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _menu?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not NtrTaskProviderState state)
            return;

        _menu?.UpdateEntries(state.Tasks, state.History, state.UntilNextSkip);
    }
}
