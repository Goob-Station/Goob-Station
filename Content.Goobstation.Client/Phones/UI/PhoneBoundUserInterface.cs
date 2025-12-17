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

        _menu.OnPhoneBookButtonPressed += i =>
        {
            SendMessage(new PhoneBookPressedMessage(i));
            _menu.DialNumber.SetMessage(i.ToString());
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_menu == null || state is not GoobPhoneBuiState s)
            return;

        Refresh(s);
    }

    private void Refresh(GoobPhoneBuiState state)
    {
        if (_menu == null)
            return;

        _menu.ClearPhoneBook();

        foreach (var phone in state.Phones)
        {
            _menu.AddPhoneBookLabel(
                phone.Name,
                phone.Category,
                phone.Number
            );
        }
    }
}
