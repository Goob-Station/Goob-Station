using Content.Goobstation.Shared.Slasher.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Slasher.UI;

[UsedImplicitly]
public sealed class SlasherKitSelectBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SlasherKitSelectMenu? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SlasherKitSelectMenu>();
        _window.OnKitSelected += index =>
        {
            SendMessage(new SlasherKitSelectedMessage(index));
            _window.ForceClose();
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is SlasherKitSelectBoundUserInterfaceState s)
            _window?.UpdateState(s);
    }
}
