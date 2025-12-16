using Content.Goobstation.Shared.Phones.Components;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Phones.UI;

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
        _menu.FillPhoneBook();

        _menu.OnKeypadButtonPressed += i =>
        {
            SendMessage(new PhoneKeypadMessage(i));

            var current = _menu.DialNumber.GetMessage();
            _menu.DialNumber.SetMessage(current + i.ToString());
        };
        _menu.OnEnterButtonPressed += () =>
        {
            SendMessage(new PhoneDialedMessage());
        };
        _menu.OnClearButtonPressed += () =>
        {
            SendMessage(new PhoneKeypadClearMessage());
            _menu.DialNumber.SetMessage(string.Empty);
        };
    }
}
