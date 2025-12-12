using Content.Goobstation.Client.Phones.UI;
using Robust.Client.UserInterface;

public sealed class PhoneBoundUserInterface : BoundUserInterface
{
    private PhoneMenu? _menu;

    public PhoneBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<PhoneMenu>();

        // Hook UI events → send messages to server
        _menu.DialButton.OnPressed += _ =>
        {
            Logger.Debug("pp poo poo");
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu == null)
            return;

        switch (state)
        {

        }
    }
}
