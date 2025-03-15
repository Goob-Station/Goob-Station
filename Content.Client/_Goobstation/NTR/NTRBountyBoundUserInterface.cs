using Content.Client._Goobstation.NTR;
using Content.Shared._Goobstation.NTR;
using Content.Shared.Cargo.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Goobstation.NTR;

[UsedImplicitly]
public sealed class NTRBountyBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NTRBountyMenu? _menu;

    public NTRBountyBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<NTRBountyMenu>();

        _menu.OnLabelButtonPressed += id =>
        {
            SendMessage(new BountyPrintLabelMessage(id));
        };

        _menu.OnSkipButtonPressed += id =>
        {
            SendMessage(new BountySkipMessage(id));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not NTRBountyConsoleState state)
            return;

        _menu?.UpdateEntries(state.Bounties, state.History, state.UntilNextUpdate);
    }
}
