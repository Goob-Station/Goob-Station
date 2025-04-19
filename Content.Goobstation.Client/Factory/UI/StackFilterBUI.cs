using Content.Goobstation.Shared.Factory.Filters;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Factory.UI;

public sealed class StackFilterBUI : BoundUserInterface
{
    private StackFilterWindow? _window;

    public StackFilterBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<StackFilterWindow>();
        _window.SetEntity(Owner);
        _window.OnSetSize += size => SendPredictedMessage(new StackFilterSetSizeMessage(size));
    }
}
