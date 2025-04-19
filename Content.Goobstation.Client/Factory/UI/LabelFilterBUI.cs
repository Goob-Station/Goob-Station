using Content.Goobstation.Shared.Factory.Filters;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Factory.UI;

public sealed class LabelFilterBUI : BoundUserInterface
{
    private LabelFilterWindow? _window;

    public LabelFilterBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<LabelFilterWindow>();
        _window.SetEntity(Owner);
        _window.OnSetLabel += label => SendPredictedMessage(new LabelFilterSetLabelMessage(label));
    }
}
