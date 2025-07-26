using Content.Goobstation.Shared.Werewolf.UI;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Werewolf.UI;

public sealed class MutationBoundUserInterface : BoundUserInterface
{
    private MutationMenu? _menu;

    public MutationBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MutationMenu>();
        _menu.SetEntity(Owner);
        _menu.Closed += OnClosed;

        _menu.OpenCentered();
    }

    private void OnClosed()
    {
        SendPredictedMessage(new ClosedMessage());
        Close();
    }
}
